using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatterService.Model;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Caching;
using System.Net;
using System.Data;

namespace ChatterService
{

    public class ChatterService : IChatterService
    {
        public const string TEST_SERVICE_URL = "https://login.ucsf--ctsi.cs10.my.salesforce.com/services/Soap/c/22.0";

        private Salesforce.SforceService _service;
        public string Url { get; set; }

        public ChatterService(string url)
        {
            Url = url;
        }

        public bool Login(string username, string password, string token)
        {
            _service = new Salesforce.SforceService();
            _service.Url = Url;

            Salesforce.LoginResult result;
            try
            {
                result = _service.login(username, password + token);

                _service.Url = result.serverUrl;
                _service.SessionHeaderValue = new Salesforce.SessionHeader();
                _service.SessionHeaderValue.sessionId = result.sessionId;
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static bool customCertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public void AllowUntrustedConnection()
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customCertificateValidation);
        }

        public void CreateActivity(string userId, string code, string message, DateTime timestamp)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            var news = new Salesforce.FeedItem
            {
                ParentId = userId,
                CreatedById = userId,
                Type = "TextPost",
                Title = code,
                Body = message,
                CreatedDate = timestamp,
                LastModifiedDate = timestamp,

            };
            var result = _service.create(new Salesforce.sObject[] { news });
        }

        public void CreateActivityUsingApex(string userId, string code, string message, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            DateTime utc = timestamp.ToUniversalTime();
            String apex = "FeedItem post = new FeedItem(); \n" +
                    " post.ParentId = '" + userId  + "'; \n" +
                    " post.Title = '" + EncodeString(code) + "'; \n" +
                    " post.Body = '" + EncodeString(message) + "'; \n" +
                    " post.CreatedById = '" + userId + "'; \n" +
                    " post.CreatedDate = Datetime.newInstanceGmt(" + utc.Year + "," + utc.Month + "," + utc.Day + "," + utc.Hour + "," + utc.Minute + "," + utc.Second + "); \n" +
                    " insert post;";
            ExecuteApex(apex);
        }

        public void CreateProfileActivity(string employeeId, string code, string message, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            Salesforce.Research_Profile__c profile = GetOrCreateResearchProfile(employeeId);

            DateTime utc = timestamp.ToUniversalTime();
            String apex = "FeedItem post = new FeedItem(); \n" +
                    " post.ParentId = '" + profile.Id + "'; \n" +
                    " post.Title = '" + EncodeString(code) + "'; \n" +
                    " post.Body = '" + EncodeString(message) + "'; \n" +
                    " post.CreatedById = '" + profile.Owner.Id + "'; \n" +
                    " post.CreatedDate = Datetime.newInstanceGmt(" + utc.Year + "," + utc.Month + "," + utc.Day + "," + utc.Hour + "," + utc.Minute + "," + utc.Second + "); \n" +
                    " insert post;";

            ExecuteApex(apex);
        }

        private void ExecuteApex(string apex)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            Salesforce.apex.ApexService service = new Salesforce.apex.ApexService();
            int idx1 = _service.Url.IndexOf(@"/services/");
            int idx2 = service.Url.IndexOf(@"/services/");
            service.Url = _service.Url.Substring(0, idx1) + service.Url.Substring(idx1);

            service.SessionHeaderValue = new Salesforce.apex.SessionHeader();
            service.SessionHeaderValue.sessionId = _service.SessionHeaderValue.sessionId;

            Salesforce.apex.ExecuteAnonymousResult result = service.executeAnonymous(apex);
            if (!result.success)
            {
                throw new Exception("Cannot create FeedItem:" + result.exceptionMessage + "\n" + result.exceptionStackTrace);
            }
        }

        public List<Activity> GetActivities(string userId)
        {
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_USER_ACTIVITIES, userId));
            var feeds = new List<Activity>();
            if (qr.size <= 0)
            {
                return feeds;
            }
            feeds.AddRange(from Salesforce.UserProfileFeed record in qr.records
                           select new Activity
                           {
                               Id = record.Id,
                               Message = record.Body,
                               CreatedDT = record.CreatedDate,
                               CreatedById = record.CreatedById
                           });

            return feeds;
        }

        public List<Activity> GetActivities(int count)
        {
            var feeds = new List<Activity>();
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_ALL_USER_ACTIVITIES, count));
            feeds.AddRange(from Salesforce.UserFeed record in qr.records
                           select new Activity
                           {
                               Id = record.Id,
                               Message = record.Body,
                               CreatedDT = record.CreatedDate,
                               CreatedById = record.CreatedById,
                               Type = ActivityType.TextPost,
                               Parent = new User
                               {
                                   Id = record.ParentId,
                                   Name = record.Parent.Name,
                                   FirstName = record.Parent.FirstName,
                                   LastName = record.Parent.LastName,
                                   EmployeeId = record.Parent.UCSF_ID__c,
                                   PersonId = GetPersonId(record.Parent.UCSF_ID__c)
                               }

                           });

            return feeds;
        }

        public List<Activity> GetProfileActivities(int count) {
            var feeds = new SortedDictionary<int, Activity>();
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_PROFILE_ACTIVITIES, 10000));

            Dictionary<int, HashSet<string>> items = new Dictionary<int, HashSet<string>>();

            Random random = new Random();
            bool done = false;
            while (!done)
            {
                for (int i = 0; i < qr.records.Length && feeds.Count < count; i++)
                {
                    Salesforce.Research_Profile__Feed record = (Salesforce.Research_Profile__Feed)qr.records[i];
                    int personId = GetPersonId(record.Parent.User__r.UCSF_ID__c);
                    HashSet<string> userPosts;
                    if (!items.TryGetValue(personId, out userPosts))
                    {
                        userPosts = new HashSet<string>();
                        items.Add(personId, userPosts);
                    }

                    
                    if(!userPosts.Contains(record.Body)) {
                        Activity act = new Activity
                        {
                            Id = record.Id,
                            Message = record.Body,
                            CreatedDT = record.CreatedDate,
                            CreatedById = record.CreatedById,
                            Type = ActivityType.TextPost,
                            Parent = new User
                            {
                                Id = record.ParentId,
                                Name = record.Parent.User__r.Name,
                                FirstName = record.Parent.User__r.FirstName,
                                LastName = record.Parent.User__r.LastName,
                                PersonId = personId
                            }
                        };

                        userPosts.Add(act.Message);
                        int key = random.Next(1000);
                        while (feeds.ContainsKey(key))
                        {
                            key = random.Next(1000);
                        }
                        feeds.Add(key, act);
                    }

                }
                if (qr.done || feeds.Count >= count)
                {
                    done = true;
                }
                else
                {
                    qr = _service.queryMore(qr.queryLocator);
                }
            }
            return feeds.Values.ToList();
        }

        public Activity[] GetProfileActivities(int count, Cache cache, int cacheInterval)
        {
            lock (this)
            {
                Activities activities = cache["ProfileActivities"] as Activities;
                if (activities == null || activities.RequestedCount < count)
                {
                    activities = new Activities(count, QueryActivitiesFromSF(count));
                    cache.Insert("ProfileActivities", activities, null, DateTime.Now.AddSeconds(cacheInterval), Cache.NoSlidingExpiration);
                }
                return activities.GetRandomList(count);
            }
        }

        //return activities from SF grouped by body
        protected List<Activity> QueryActivitiesFromSF(int count)
        {            
            List<Activity> activities = new List<Activity>();
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_PROFILE_ACTIVITIES, 10000));

            Dictionary<int, HashSet<string>> items = new Dictionary<int, HashSet<string>>();
            bool done = false;
            while (!done)
            {
                for (int i = 0; i < qr.records.Length && activities.Count < count; i++)
                {
                    Salesforce.Research_Profile__Feed record = (Salesforce.Research_Profile__Feed)qr.records[i];
                    int personId = GetPersonId(record.Parent.User__r.UCSF_ID__c);
                    HashSet<string> userActivities;
                    if (!items.TryGetValue(personId, out userActivities))
                    {
                        userActivities = new HashSet<string>();
                        items.Add(personId, userActivities);
                    }


                    if (!userActivities.Contains(record.Body))
                    {
                        Activity act = new Activity
                        {
                            Id = record.Id,
                            Message = record.Body,
                            CreatedDT = record.CreatedDate,
                            CreatedById = record.CreatedById,
                            Type = ActivityType.TextPost,
                            Parent = new User
                            {
                                Id = record.ParentId,
                                Name = record.Parent.User__r.Name,
                                FirstName = record.Parent.User__r.FirstName,
                                LastName = record.Parent.User__r.LastName,
                                PersonId = personId
                            }
                        };

                        userActivities.Add(act.Message);
                        activities.Add(act);
                    }

                }
                if (qr.done || activities.Count >= count)
                {
                    done = true;
                }
                else
                {
                    qr = _service.queryMore(qr.queryLocator);
                }
            }
            return activities;
        }

        protected void CreateResearchProfileInternal(string employeeId)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(employeeId))
                throw new Exception("Invalid argument!");

            Salesforce.User user = GetUser(employeeId);

            Salesforce.Research_Profile__c profile = new Salesforce.Research_Profile__c
            {
                Name = user.FirstName + " " + user.LastName,
                OwnerId = user.Id,
                User__c = user.Id

            };
            Salesforce.SaveResult[] results = _service.create(new Salesforce.sObject[] { profile });
            foreach (Salesforce.SaveResult result in results)
            {
                if (!result.success)
                {
                    throw new Exception("Cannot create ResearchProfile for employee id=" + employeeId + ", with error:\n" + result.errors.Select(it => it.statusCode + ":" + it.message).Aggregate((s1, s2) => s1 + "\n" + s2));
                }
            }

        }

        public void CreateResearchProfile(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }

            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_RESEACH_PROFILE, employeeId));
            if (qr.records == null)
            {
                CreateResearchProfileInternal(employeeId);
            }
        }

#region "Get Salesforce objects"

        public string GetUserId(string employeeId)
        {
            Salesforce.User user = GetUser(employeeId);
            return user.Id;
        }

        public Salesforce.User GetUser(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }

            return GetObject<Salesforce.User>(Queries.SOQL_GET_USER, employeeId);
        }

        protected Salesforce.Research_Profile__c GetResearchProfile(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }
            return GetObject<Salesforce.Research_Profile__c>(Queries.SOQL_GET_RESEACH_PROFILE, employeeId); 
        }

        protected Salesforce.Research_Profile__c GetOrCreateResearchProfile(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }

            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_RESEACH_PROFILE, employeeId));
            if (qr.records == null)
            {
                CreateResearchProfileInternal(employeeId);
                return GetResearchProfile(employeeId);
            }

            return (Salesforce.Research_Profile__c)qr.records.FirstOrDefault();
        }

        protected T GetObject<T>(string query, params string[] pk) where T : Salesforce.sObject
        {
            Salesforce.QueryResult qr = _service.query(string.Format(query, pk));
            if (qr.records == null)
            {
                throw new Exception(string.Format("Object not found, {0}, keys:{1}", typeof(T).FullName, pk.Select(it => it.ToString()).Aggregate((s1, s2) => s1 + "," + s2)));
            }
            var profile = qr.records.FirstOrDefault();
            return (T)profile;
        }
#endregion

        public static int GetPersonId(string employeeId)
        {
            int id = 2569307 + Int32.Parse(employeeId.Substring(1, 7));
            return id;
        }

        protected String EncodeString(string str)
        {
            return str.Replace("'", "\\'");
        }

    }
}
