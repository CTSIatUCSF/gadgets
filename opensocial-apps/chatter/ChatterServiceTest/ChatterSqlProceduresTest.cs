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
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>-2147483648</postedTime><title>Test Activity created by ChatterSqlProceduresTest.TestCreateActivity</title></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob);
        }

        [TestMethod]
        public void TestCreateActivityWithMissingMessageElement()
        {
            string xml =
                "<activity xmlns=\"http://ns.opensocial.org/2008/opensocial\"><postedTime>1304533194580</postedTime><title2>This is Profiles test activity from ChatterSqlProceduresTest</title2></activity>";
            var xmlReader = XmlTextReader.Create(new System.IO.StringReader(xml));

            SqlXml messageBlob = new SqlXml(xmlReader);

            try
            {
                ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, _employeeId, messageBlob);
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
                ChatterSqlProcedures.CreateActivity(_url, _username, _password, _token, "1221212", messageBlob);
                Assert.Fail("CreateActivity method should throw and exception if user is not found by employee id");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Cannot find user with employeeId=1221212", ex.Message);
            }
        }

        [TestMethod]
        public void TestConvertUnixEpochTime()
        {
            DateTime dt = ChatterSqlProcedures.ConvertUnixEpochTime("1304641078588");
            Assert.AreEqual(new DateTime(2011, 5, 5, 16, 17, 58, 588, DateTimeKind.Utc), dt);

            ChatterSqlProcedures.ConvertUnixEpochTime("-2147483648");
            ChatterSqlProcedures.ConvertUnixEpochTime("1304705930906.25");
        }

    }
}
