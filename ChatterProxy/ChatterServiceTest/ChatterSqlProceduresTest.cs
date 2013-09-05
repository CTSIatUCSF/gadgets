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
using System.Configuration;

namespace ChatterServiceTest
{
    [TestClass]
    public class ChatterSqlProceduresTest
    {
        string _url = ConfigurationSettings.AppSettings["services_url"]; 
        string _username = ConfigurationSettings.AppSettings["username"];
        string _password = ConfigurationSettings.AppSettings["password"];
        string _token = ConfigurationSettings.AppSettings["token"];
        int _pmid = 1234;
        string _title = "Book Title";
        string _body = "Book Body";

        string _employeeId = "111111111";

        [TestMethod]
        public void TestCreateActivity()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1310597396000</postedTime><title>Edited their narrative</title><body>Test Activity created by ChatterSqlProceduresTest.TestCreateActivity</body></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            //ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob, _pmid, _title, _body);
        }

        [TestMethod]
        public void TestCreateActivityWithMissingBody()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1310597396000</postedTime><title>Edited their narrative</title></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            //ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob, _pmid, _title, _body);
        }

        [TestMethod]
        public void TestCreateActivityWithMissingMessageElement()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1304533194580</postedTime><body>This is Profiles test activity from ChatterSqlProceduresTest</body></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            try
            {
                //ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob, _pmid, _title, _body);
                Assert.Fail("CreateActivity method should throw and exception if xml is incorrect");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Element {http://ns.opensocial.org/2008/opensocial}title was not found", ex.Message);
            }
        }

        [TestMethod]
        public void TestCreateActivityWithIncorrectEmployee()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1304533194580</postedTime><title>This is Profiles test activity from ChatterSqlProceduresTest</title></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            try
            {
                //ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, "1221212", messageBlob, _pmid, _title, _body);
                Assert.Fail("CreateActivity method should throw and exception if user is not found by employee id");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Object not found, Salesforce.User, keys:1221212", ex.Message);
            }
        }

    }
}
