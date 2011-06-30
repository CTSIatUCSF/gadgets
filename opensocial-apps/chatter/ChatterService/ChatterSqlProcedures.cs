using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace ChatterService
{
    public class ChatterSqlProcedures
    {

        public ChatterSqlProcedures()
        {
        }

        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        [SqlProcedure]
        public static void CreateActivity(SqlString url, SqlString username, SqlString password, SqlString token, SqlString employId, SqlXml messageBlob)
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);

            IChatterService service = new ChatterService(url.Value);
            service.Login(username.Value, password.Value, token.Value);
            
            var element = XElement.Parse(messageBlob.Value);
            
            var message = GetElementValue(element, "{http://ns.opensocial.org/2008/opensocial}title");

            var postedTimeStr = GetElementValue(element, "{http://ns.opensocial.org/2008/opensocial}postedTime"); ;
            long postedTimeLong = 0;
            long.TryParse(postedTimeStr, out postedTimeLong);

            var userId = service.GetUserId(employId.Value);
            service.CreateActivity(userId, message, new DateTime(postedTimeLong));
        }

        private static string GetElementValue(XElement element, string name)
        {
            if (string.IsNullOrEmpty(name) || element == null || element.Element(name) == null)
                return string.Empty;
            return element.Element(name).Value;
        }

    }
}
