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

namespace ChatterService.Web
{
    [ServiceContract(Name = "ChatterProxyService")]
    public interface IChatterProxyService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/activities?count={count}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetActivities(int count);

        [OperationContract]
        [WebGet(UriTemplate = "/user/{userId}/activities", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Activity[] GetUserActivities(string userId);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                 ConcurrencyMode = ConcurrencyMode.Single, 
                 IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ChatterProxyService : IChatterProxyService
    {
        string url;
        string userName;
        string password;
        string token;

        public ChatterProxyService()
        {
            url = ConfigurationSettings.AppSettings["SalesForceUrl"];
            userName = ConfigurationSettings.AppSettings["SalesForceUserName"];
            password = ConfigurationSettings.AppSettings["SalesForcePassword"];
            token = ConfigurationSettings.AppSettings["SalesForceToken"];
        }

        public Activity[] GetActivities(int count)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);

            IChatterService service = new ChatterService(url);
            service.Login(userName, password, token);
            Activity[] list = service.GetProfileActivities(count).ToArray();
//            JavaScriptSerializer js = new JavaScriptSerializer();
//            string strJSON = js.Serialize(list);

            return list;
        }

        public Activity[] GetUserActivities(string userId)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);

            IChatterService service = new ChatterService(url);
            service.Login(userName, password, token);

            var ssUserId = service.GetUserId(userId);
            return service.GetActivities(ssUserId).ToArray();
        }

        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
    }

   
}