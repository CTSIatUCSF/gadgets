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
using DevDefined.OAuth;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Framework.Signing;
using System.Security.Cryptography;
using System.Web.Caching;

namespace ChatterService.Web
{
    public class CommonResult {
        public bool Success {get; set;}
        public bool Following { get; set; }
        public string ErrorMessage { get; set; }
        public int Total { get; set; }
        public string AccessToken { get; set; }
        public string URL { get; set; }
    }

    [ServiceContract(Name = "ChatterProxyService")]
    public interface IChatterProxyService   
    {
        [OperationContract]
        [WebGet(UriTemplate = "/activities?count={count}&mode={mode}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetActivities(int count, string mode);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{personId}/activities?count={count}&mode={mode}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetUserActivities(string personId, string mode, int count);

        [OperationContract]
        [WebInvoke(UriTemplate = "/group/new", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CommonResult CreateGroup(Stream stream);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{viewerId}/isfollowing/{ownerId}?accessToken={accessToken}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CommonResult IsUserFollowing(string viewerId, string ownerId, string accessToken);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{viewerId}/follow/{ownerId}?accessToken={accessToken}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CommonResult Follow(string viewerId, string ownerId, string accessToken);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{viewerId}/unfollow/{ownerId}?accessToken={accessToken}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CommonResult Unfollow(string viewerId, string ownerId, string accessToken);
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
        readonly string clientId;
        readonly string grantType;
        readonly string clientSecret;
        readonly int cacheInterval;
        readonly int cacheCapacity;
        readonly bool logService;

        readonly bool signedFetch;
        OAuthContextSigner signer;
        SigningContext signingContext;
        AsymmetricAlgorithm provider;

        bool fetchingActivities = false;
        Timer activitiesFetcher;
        DeclumpedList latestList = new DeclumpedList();

        IProfilesServices profilesService = null;

        public ChatterProxyService()
        {
            WriteLogToFile("Starting ChatterProxyService");
            url = ConfigurationSettings.AppSettings["SalesForceUrl"];
            userName = ConfigurationSettings.AppSettings["SalesForceUserName"];
            password = ConfigurationSettings.AppSettings["SalesForcePassword"];
            token = ConfigurationSettings.AppSettings["SalesForceToken"];
            clientId = ConfigurationSettings.AppSettings["SalesForceClientId"];
            grantType = ConfigurationSettings.AppSettings["SalesForceGrantType"];
            clientSecret = ConfigurationSettings.AppSettings["SalesForceClientSecret"];
            cacheInterval = Int32.Parse(ConfigurationSettings.AppSettings["CacheInterval"]);
            cacheCapacity = Int32.Parse(ConfigurationSettings.AppSettings["cacheCapacity"]);
            logService = Boolean.Parse(ConfigurationSettings.AppSettings["LogService"]);
            signedFetch = Boolean.Parse(ConfigurationSettings.AppSettings["SignedFetch"]);

            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);
            profilesService = new ProfilesServices();
            getChatterSoapService();

            if (signedFetch)
            {
                // load default cert
                X509Certificate2 cert = new X509Certificate2(ConfigurationSettings.AppSettings["OAuthCert"]);
                provider = cert.PublicKey.Key;
                signer = new OAuthContextSigner();
                signingContext = new SigningContext();
                //signingContext.ConsumerSecret = ...; // if there is a consumer secret
                signingContext.Algorithm = provider;
            }

            activitiesFetcher = new Timer(GetActivities, null, 0, cacheInterval * 1000);
        }

        public Activity[] GetActivities(int count, string mode)
        {
            try
            {
                if ("chronological".Equals(mode))
                {
                    lock (latestList)
                    {
                        return latestList.Take(count).ToArray();
                    }
                }
                else
                { // declump
                    lock (latestList)
                    {
                        return latestList.TakeUnclumped(count).ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(e, url);
                return null;
            }
        }

        public void GetActivities(Object stateInfo)
        {
            // this IS thread safe, but very hokey. Just don't want to pile up a bunch of threads in here
            if (fetchingActivities)
            {
                return;
            }
            fetchingActivities = true;
            try
            {
                Activity lastActivity = latestList.Count > 0 ? latestList[0] : null;

                ChatterSoapService soap = getChatterSoapService();
                List<Activity> newActivities = soap.GetProfileActivities(lastActivity, cacheCapacity);
                if (newActivities.Count > 0)
                {
                    WriteLogToFile("Adding " + newActivities.Count + " new activities to list");
                    lock (latestList)
                    {
                        latestList.AddRange(newActivities);
                        latestList.Sort(new ActivitiesComparer());
                        if (latestList.Count > cacheCapacity)
                        {
                            latestList.RemoveRange(cacheCapacity, latestList.Count - cacheCapacity);
                        }
                        latestList.Clump();
                    }
                    WriteLogToFile("List count is now " + latestList.Count);
                }
            }
            catch (Exception e)
            {
                HandleError(e, url);
            }
            finally
            {
                fetchingActivities = false;
            }
        }

        public Activity[] GetUserActivities(string userId, string mode, int count)
        {
            try {
                ValidateSignature();
                bool includeUserActivities = mode.Equals("all", StringComparison.InvariantCultureIgnoreCase);

                ChatterSoapService soap = getChatterSoapService();
                var ssUserId = getSalesforceUserId(soap, userId);
                Activity[] result = soap.GetActivities(ssUserId, Int32.Parse(userId), includeUserActivities, count).ToArray();
                return result;
            }
            catch (Exception e)
            {
                HandleError(e, url);
                return null;
            }
        }

        public CommonResult CreateGroup(Stream stream)
        {
            try {
                ValidateSignature();
                NameValueCollection p = parseParameters(stream);

                if (string.IsNullOrEmpty(p["name"]))
                {
                    return new CommonResult() { Success = false, ErrorMessage = "Group name is required." };
                }

                if (string.IsNullOrEmpty(p["ownerId"]))
                {
                    return new CommonResult() { Success = false, ErrorMessage = "OwnerId is required." };
                }

                string nodeId = p["ownerId"];
                string descr = p["description"];
                if (string.IsNullOrEmpty(descr))
                {
                    descr = p["name"];
                }

                string employeeId = profilesService.GetEmployeeId(nodeId);

                ChatterSoapService soap = getChatterSoapService();
                string groupId = soap.CreateGroup(p["name"], descr, employeeId);

                string users = p["users"];
                if (!string.IsNullOrEmpty(users))
                {
                    string[] personList = users.Split(',');
                    List<string> employeeList = new List<string>();
                    foreach (string pId in personList)
                    {
                        try
                        {
                            string eId = profilesService.GetEmployeeId(pId);
                            employeeList.Add(eId);
                        }
                        catch (Exception ex)
                        {
                            //TODO: need to report it back to the server
                        }
                    }

                    if (employeeList.Count > 0)
                    {
                        soap.AddUsersToGroup(groupId, employeeList.ToArray<string>());
                    }
                }
                return new CommonResult() { Success = true, URL = url.Replace("/services", "/_ui/core/chatter/groups/GroupProfilePage?g=" + groupId) };
            }
            catch (Exception ex)
            {
                HandleError(ex, url);
                return new CommonResult() { Success = false, ErrorMessage = ex.Message };
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

        #region REST
        public CommonResult IsUserFollowing(string viewerId, string ownerId, string accessToken)
        {
            try
            {
                ChatterSoapService soap = getChatterSoapService();
                var ssOwnerId = getSalesforceUserId(soap, ownerId);
                var ssViewerId = getSalesforceUserId(soap, viewerId);

                ChatterRestService rest = getChatterRestService(accessToken);
                ChatterResponse cresp = rest.GetFollowers(ssOwnerId);
                int total = cresp != null ? cresp.total : 0;

                while (cresp != null && cresp.followers != null)
                {
                    foreach (ChatterSubscription csub in cresp.followers)
                    {
                        if (csub.subscriber != null && csub.subscriber.id.StartsWith(ssViewerId))
                        {
                            return new CommonResult() { Success = true, Following = true, Total = total, AccessToken = rest.GetAccessToken() };
                        }
                    }
                    cresp = rest.GetNextPage(cresp);
                }

                return new CommonResult() { Success = true, Following = false, Total = total, AccessToken = rest.GetAccessToken() };
            }
            catch (Exception ex)
            {
                HandleError(ex, url);
                HandleError(ex, accessToken);
                return new CommonResult() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public CommonResult Follow(string viewerId, string ownerId, string accessToken)
        {
            try
            {
                ValidateSignature();
                ChatterSoapService soap = getChatterSoapService();
                var ssOwnerId = getSalesforceUserId(soap, ownerId);
                var ssViewerId = getSalesforceUserId(soap, viewerId);

                ChatterRestService rest = getChatterRestService(accessToken);
                ChatterResponse cresp = rest.Follow(ssViewerId, ssOwnerId);
                return IsUserFollowing(viewerId, ownerId, accessToken);
            }
            catch (Exception ex)
            {
                HandleError(ex, url);
                HandleError(ex, accessToken);
                return new CommonResult() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public CommonResult Unfollow(string viewerId, string ownerId, string accessToken)
        {
            if (logService)
            {
                WriteLogToFile(viewerId + "/unfollow/" + ownerId + "?accessToken=" + accessToken);
            }
            try
            {
                ValidateSignature();
                ChatterSoapService soap = getChatterSoapService();
                var ssOwnerId = getSalesforceUserId(soap, ownerId);
                var ssViewerId = getSalesforceUserId(soap, viewerId);

                ChatterRestService rest = getChatterRestService(accessToken);
                ChatterResponse cresp = rest.Unfollow(ssViewerId, ssOwnerId);
                return IsUserFollowing(viewerId, ownerId, accessToken);
            }
            catch (Exception ex)
            {
                HandleError(ex, url);
                HandleError(ex, accessToken);
                return new CommonResult() { Success = false, ErrorMessage = ex.Message };
            }
        }
        #endregion

        private string getSalesforceUserId(ChatterSoapService soap, string nodeId)
        {
            Object objUserId = HttpRuntime.Cache[nodeId];

            if (objUserId == null)
            {
                objUserId = soap.GetUserId(profilesService.GetEmployeeId(nodeId));
                HttpRuntime.Cache.Insert(nodeId, objUserId);
            }
            return Convert.ToString(objUserId);
        }

        private ChatterSoapService getChatterSoapService()
        {
            ChatterSoapService soap = (ChatterSoapService)HttpRuntime.Cache[url];
            if (soap == null)
            {
                lock (this)
                {
                    WriteLogToFile("Refreshing ChatterSoapService");
                    soap = new ChatterSoapService(url);
                    soap.Login(userName, password, token);
                    WriteLogToFile("Success in logging into ChatterSoapService");
                    HttpRuntime.Cache.Insert(url, soap);
                }
            }
            return soap;
        }

        private ChatterRestService getChatterRestService(string accessToken)
        {
            ChatterRestService rest = accessToken !=  null && accessToken.Trim().Length > 0 ? (ChatterRestService)HttpRuntime.Cache[accessToken] : null;

            if (rest == null)
            {
                rest = new ChatterRestService(url, logService);
                accessToken = rest.Login(clientId, grantType, clientSecret, userName, password);
                HttpRuntime.Cache.Insert(accessToken, rest);
            }
            return rest;
        }

        #region IErrorHandler Members
        public bool HandleError(Exception error, string key)
        {
            WriteLogToFile(error.Message);

            // remove what we think may be stale
            if (key != null && key.Trim().Length > 0)
            {
                // force the ChatterSoapService to logout in this situation.
                Object o = HttpRuntime.Cache.Remove(key);
            }
            // Returning true indicates you performed your behavior. 
            return true;
        }
        #endregion    

        private void WriteLogToFile(String msg)
        {
            try
            {

                using (StreamWriter w = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "/ChatterProxyService.txt"))
                {
                    // write a line of text to the file
                    w.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffff"));
                    w.WriteLine("\t" + msg);

                    // close the stream
                    w.Close();

                }
            }
            catch (Exception ex) { throw ex; }
        }

        private void ValidateSignature()
        {
            if (!signedFetch)
            {
                return;
            }

            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;

            IOAuthContext context = new OAuthContextBuilder().FromUri(request.Method, request.UriTemplateMatch.RequestUri);                        

            // use context.ConsumerKey to fetch information required for signature validation for this consumer.
            if (!signer.ValidateSignature(context, signingContext)) 
            {
                throw new Exception("Invalid signature : " + request.UriTemplateMatch.RequestUri);
            }
        }

        ~ChatterProxyService()
        {
            WriteLogToFile("Shutting down ChatterProxyService");
            if (activitiesFetcher != null)
            {
                activitiesFetcher.Dispose();
            }
        }
    }
   
}