using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ChatterService;
using System.Configuration;

namespace ChatterServiceTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ChatterRestServiceTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        string _url = ConfigurationSettings.AppSettings["services_url"];
        string _client_id = ConfigurationSettings.AppSettings["client_id"];
        string _grant_type = ConfigurationSettings.AppSettings["grant_type"];
        string _client_secret = ConfigurationSettings.AppSettings["client_secret"];
        string _username = ConfigurationSettings.AppSettings["username"];
        string _password = ConfigurationSettings.AppSettings["password"];
        

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ChatterServiceTestInitialize(TestContext testContext)
        {
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        //public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestRestLogin()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
        }

        [TestMethod]
        public void TestRestGetFollowers()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.GetFollowers("005A0000001X3Nb");
        }

        [TestMethod]
        public void TestRestGetFollowing()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.GetFollowing("005A0000001X3Nb");
        }

        [TestMethod]
        public void TestRestFollow()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.Follow("005A0000001X3Nb", "005A0000001X3gU");
        }

        [TestMethod]
        public void TestRestUnfollow()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.Unfollow("005A0000001X3Nb", "005A0000001X3gUIAS");
        }
    }
}
