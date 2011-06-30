using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChatterService;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace ChatterServiceTest
{
    [TestClass]
    public class ChatterSqlProceduresTest
    {
        string _url = ChatterService.ChatterService.TEST_SERVICE_URL;
        string _username = "alexnv@oneorg.ucsf.edu.ctsi";
        string _password = "";
        string _token = "MQzWKEZxvtNXrHM0X8hcHbzPI";

        string _employeeId = "111111111";

        [TestMethod]
        public void TestCreateActivity()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1304533194580</postedTime><title>This is Profiles test activity from ChatterSqlProceduresTest</title></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob);
        }
    }
}
