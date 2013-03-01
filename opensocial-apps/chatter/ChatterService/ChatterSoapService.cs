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

    public class ChatterSoapService : IChatterSoapService
    {
        private Salesforce.SforceService _service;
        private String _userId;
        public string Url { get; set; }

        public ChatterSoapService(string url)
        {
            Url = url + "/Soap/c/22.0";
        }

        public bool Login(string username, string password, string token)
        {
            _service = new Salesforce.SforceService();
            _service.Url = Url;

            Salesforce.LoginResult result;
            try
            {
                result = _service.login(username, password + token);

                _userId = result.userId;
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
            service.Url = _service.Url.Substring(0, idx1) + service.Url.Substring(idx2);

            service.SessionHeaderValue = new Salesforce.apex.SessionHeader();
            service.SessionHeaderValue.sessionId = _service.SessionHeaderValue.sessionId;

            Salesforce.apex.ExecuteAnonymousResult result = service.executeAnonymous(apex);
            if (!result.success)
            {
                throw new Exception("Cannot create FeedItem:" + result.compileProblem + "\n" + result.exceptionStackTrace);
            }
        }

        public List<Activity> GetActivities(string userId, int personId, bool includeUserActivities, int count)
        {
            List<Activity> result = GetProfileActivities(userId, count);
            if (includeUserActivities)
            {
                List<Activity> userActivities = GetUserActivities(userId, personId, count);
                result.AddRange(userActivities);
                result.Sort(new ActivitiesComparer());
            }

            return result;
        }

        public List<Activity> GetUserActivities(string userId, int personId, int count)
        {
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_USER_ACTIVITIES, userId, count));
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
                               CreatedDT = (DateTime)record.CreatedDate,
                               CreatedById = record.CreatedById,
                               Parent = new User
                               {
                                    Id = record.ParentId,
                                    Name = record.Parent.Name1,
                                    FirstName = record.Parent.FirstName,
                                    LastName = record.Parent.LastName,
                                    PersonId = personId
                               }
                           });

            return feeds;
        }

        public List<Activity> GetProfileActivities(string userId, int count) {
            var feeds = new List<Activity>();
            Salesforce.QueryResult qr = _service.query(string.Format(Queries.SOQL_GET_PROFILE_ACTIVITIES_BY_USER, userId, count > 10000 ? 10000 : count));
            if (qr.size <= 0)
            {
                return feeds;
            }

            feeds.AddRange(from Salesforce.Research_Profile__Feed record in qr.records
                           where (record.Title != "profile was viewed" && record.Title != "gadget was viewed")
               select new Activity
               {
                   Id = record.Id,
                   Message = record.Body,
                   CreatedDT = (DateTime)record.CreatedDate,
                   CreatedById = record.CreatedById,
                   Parent = new User
                   {
                        Id = record.ParentId,
                        Name = record.Parent.User__r.Name,
                        FirstName = record.Parent.User__r.FirstName,
                        LastName = record.Parent.User__r.LastName,
                        PersonId = GetPersonId(record.Parent.User__r.UCSF_ID__c)
                    }
            });

            return feeds;
        }

        public List<Activity> GetProfileActivities(Activity lastActivity, int count)
        {
            Activities activities = new Activities(count, QueryActivitiesFromSF(lastActivity, count));
            return activities.items;
        }

        //return activities from SF
        protected List<Activity> QueryActivitiesFromSF(Activity lastActivity, int count)
        {            
            List<Activity> activities = new List<Activity>();
            Salesforce.QueryResult qr = (lastActivity == null ? _service.query(string.Format(Queries.SOQL_GET_PROFILE_ACTIVITIES, 10000)) :
                                                         _service.query(string.Format(Queries.SOQL_GET_PROFILE_ACTIVITIES_AFTER, lastActivity.CreatedDT.ToUniversalTime().ToString("s"), 10000)));

            bool done = false;
            while (!done)
            {
                for (int i = 0; i < qr.records.Length && activities.Count < count; i++)
                {
                    Salesforce.Research_Profile__Feed record = (Salesforce.Research_Profile__Feed)qr.records[i];

                    if (Filter(record, activities))
                    {
                        continue;
                    }

                    int personId = GetPersonId(record.Parent.User__r.UCSF_ID__c);

                    Activity act = new Activity
                    {
                        Id = record.Id,
                        Message = record.Body,
                        CreatedDT = (DateTime)record.CreatedDate,
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

                    activities.Add(act);
 
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

        private bool Filter(Salesforce.Research_Profile__Feed record, List<Activity> activities)
        {
            if (!String.IsNullOrEmpty(record.Title))
            {
                string title = record.Title.ToLower();
                if (title.Equals("profile was viewed") || title.Equals("gadget was viewed"))
                {
                    return true;
                }
            }


            for (int i = activities.Count - 1; i >= 0; i--)
            {
                DateTime date = (DateTime)record.CreatedDate;
                DateTime prevDate = activities[i].CreatedDT;
                if (date.Day != prevDate.Day || date.Month != prevDate.Month || date.Year != prevDate.Year)
                {
                    return false;
                }
                if (record.ParentId == activities[i].Parent.Id && record.Body == activities[i].Message)
                {
                    return true;
                }

            }
            return false;
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
                Name = user.FirstName + " " + user.LastName + " Research Profile",
                OwnerId = _userId,
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

        public string CreateGroup(String name, String descriptioon, string ownerEmployeeId)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ownerEmployeeId))
                throw new Exception("Invalid argument!");

            Salesforce.User user = GetUser(ownerEmployeeId);
            Salesforce.CollaborationGroup grp = new Salesforce.CollaborationGroup()
            {
                    Name = name,
                    Description = descriptioon,
                    OwnerId = user.Id,
                    CollaborationType = "Private"
            };

            Salesforce.SaveResult[] results = _service.create(new Salesforce.sObject[] { grp });
            foreach (Salesforce.SaveResult result in results)
            {
                if (!result.success)
                {
                    throw new Exception(result.errors.Select(it => it.statusCode + ":" + it.message).Aggregate((s1, s2) => s1 + "\n" + s2));
                }
            }
            return results[0].id;
        }

        public void AddUsersToGroup(String groupId, string[] users)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(groupId) || users == null || users.Length == 0)
                throw new Exception("Invalid argument!");

            Salesforce.sObject[] objects = new Salesforce.sObject[users.Length];
            for (int i = 0; i < users.Length;i++ )
            {
                Salesforce.User user = GetUser(users[i]);
                objects[i] = new Salesforce.CollaborationGroupMember()
                {
                    CollaborationGroupId = groupId,
                    MemberId = user.Id
                };
            }

            Salesforce.SaveResult[] results = _service.create(objects);
            foreach (Salesforce.SaveResult result in results)
            {
                if (!result.success)
                {
                    throw new Exception("Cannot create GroupMember for group id=" + groupId + ", with error:\n" + result.errors.Select(it => it.statusCode + ":" + it.message).Aggregate((s1, s2) => s1 + "\n" + s2));
                }
            }
        }

        public void CreateExternalMessage(string url, string title, string body, string employeeId)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(employeeId))
                throw new Exception("Invalid argument!");

            Salesforce.External_Message__c externalMessage = new Salesforce.External_Message__c
            {
                LinkURL__c = url,
                Title__c = title,
                Original_Body__c = body,
                UCSF_ID__c = employeeId                
            };

            Salesforce.SaveResult[] results = _service.create(new Salesforce.sObject[] { externalMessage });
            foreach (Salesforce.SaveResult result in results)
            {
                if (!result.success)
                {
                    throw new Exception("Cannot create ExternalMessage for employee id=" + employeeId + ", with error:\n" + result.errors.Select(it => it.statusCode + ":" + it.message).Aggregate((s1, s2) => s1 + "\n" + s2));
                }
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
