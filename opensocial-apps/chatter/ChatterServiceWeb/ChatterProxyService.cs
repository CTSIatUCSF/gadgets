using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using ChatterService.Model;
using System.Configuration;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Web.Script.Serialization;   
using System.Web.Script.Services;
using System.IO;
using System.Collections.Specialized;
using System.Threading;

namespace ChatterService.Web
{
    public class CreateResult {
        public bool Success {get; set;}
        public string ErrorMessage {get; set;}
    }

    [ServiceContract(Name = "ChatterProxyService")]
    public interface IChatterProxyService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/activities?count={count}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetActivities(int count);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{personId}/activities?count={count}&mode={mode}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetUserActivities(string personId, string mode, int count);

        [OperationContract]
        [WebInvoke(UriTemplate = "/group/new", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CreateResult CreateGroup(Stream stream);

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                 ConcurrencyMode = ConcurrencyMode.Single, 
                 IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ChatterProxyService : IChatterProxyService
    {
        readonly string url;
        readonly string userName;
        readonly string password;
        readonly string token;
        readonly int cacheInterval;
        readonly int cacheCapacity;
        static bool initialized = false;
        IChatterService _service = null; 
        Timer activitiesFetcher;
        List<Activity> latestList = new List<Activity>();
        List<Activity> displayList = new List<Activity>();

        public ChatterProxyService()
        {
            url = ConfigurationSettings.AppSettings["SalesForceUrl"];
            userName = ConfigurationSettings.AppSettings["SalesForceUserName"];
            password = ConfigurationSettings.AppSettings["SalesForcePassword"];
            token = ConfigurationSettings.AppSettings["SalesForceToken"];
            cacheInterval = Int32.Parse(ConfigurationSettings.AppSettings["CacheInterval"]);
            cacheCapacity = Int32.Parse(ConfigurationSettings.AppSettings["cacheCapacity"]);
            Init();
        }

        public Activity[] GetActivities(int count)
        {
            lock (latestList)
            {
                return latestList.Take(count).ToArray();
            }
        }

        public void GetActivities(Object stateInfo)
        {
            // login if needed
            lock (this)
            {
                try 
                {
                    if (_service == null)
                    {
                        _service = new ChatterService(url);
                        _service.Login(userName, password, token);
                    }
                    Activity lastActivity = latestList.Count > 0 ? latestList[0] : null;
                    List<Activity> newActivities = _service.GetProfileActivities(lastActivity, cacheCapacity);
                    if (newActivities.Count > 0)
                    {
                        lock (latestList)
                        {
                            latestList.AddRange(newActivities);
                            latestList.Sort(new ActivitiesComparer());
                            latestList.RemoveRange(cacheCapacity, latestList.Count - cacheCapacity);
                        }
                    }
                }
                catch (Exception e) 
                {
                    _service = null;
                }
            }
        }

        public Activity[] GetUserActivities(string userId, string mode, int count)
        {
            IProfilesServices profiles = new ProfilesServices();
            IChatterService service = new ChatterService(url);
            service.Login(userName, password, token);

            int personId = Int32.Parse(userId);
            bool includeUserActivities = mode.Equals("all", StringComparison.InvariantCultureIgnoreCase);

            string employeeId = profiles.GetEmployeeId(personId);
            var ssUserId = service.GetUserId(employeeId);
            Activity[] result = service.GetActivities(ssUserId, personId, includeUserActivities, count).ToArray();
            return result;
        }

        public CreateResult CreateGroup(Stream stream)
        {
            NameValueCollection p = parseParameters(stream);

            if (string.IsNullOrEmpty(p["name"]))
            {
                return new CreateResult() { Success = false, ErrorMessage = "Group name is required."};
            }

            if (string.IsNullOrEmpty(p["ownerId"]))
            {
                return new CreateResult() { Success = false, ErrorMessage = "OwnerId is required." };
            }

            try
            {
                int personId = Int32.Parse(p["ownerId"]);
                string descr = p["description"];
                if (string.IsNullOrEmpty(descr))
                {
                    descr = p["name"];
                }

                IProfilesServices profiles = new ProfilesServices();
                string employeeId = profiles.GetEmployeeId(personId);

                IChatterService service = new ChatterService(url);
                service.Login(userName, password, token);
                string groupId = service.CreateGroup(p["name"], descr, employeeId);

                string users = p["users"];
                if(!string.IsNullOrEmpty(users)) {
                    string[] personList = users.Split(',');
                    List<string> employeeList = new List<string>();
                    foreach (string pId in personList)
                    {
                        try
                        {
                            string eId = profiles.GetEmployeeId(Int32.Parse(pId));
                            employeeList.Add(eId);
                        }
                        catch (Exception ex)
                        {
                            //TODO: need to report it back to the server
                        }
                    }

                    if (employeeList.Count > 0)
                    {
                        service.AddUsersToGroup(groupId, employeeList.ToArray<string>());
                    }
                }

                return new CreateResult() { Success = true};
            }
            catch (Exception ex)
            {
                return new CreateResult() { Success = false, ErrorMessage = ex.Message};
            }

        }

        private NameValueCollection parseParameters(Stream stream) {
            string s = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                s = sr.ReadToEnd();
            }

            return HttpUtility.ParseQueryString(s);
        }

        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        private void Init()
        {
            lock(this) {
                if (!initialized)
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);
                    activitiesFetcher = new Timer(GetActivities, null, 0, cacheInterval * 1000);
                    initialized = true;
                }
            }
        }
    }

   
}