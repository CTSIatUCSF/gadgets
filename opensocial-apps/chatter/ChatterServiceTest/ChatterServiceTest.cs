using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ChatterService;
using ChatterService.Model;

namespace ChatterServiceTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ChatterServiceTest
    {
        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public ChatterServiceTest()
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(customXertificateValidation);

        }

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
            string _url = ChatterService.ChatterService.TEST_SERVICE_URL;
            string _username = "";
            string _password = "";
            string _token = "MQzWKEZxvtNXrHM0X8hcHbzPI";

            string _employeeId = "111111111";
            string _userId = "005J0000000Q74wIAC";
        

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
        public void TestLogin()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);
        }

        [TestMethod]
        public void TesGetUserId()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);

            string id = service.GetUserId(_employeeId);
            Assert.AreEqual(_userId, id);            
        }

        [TestMethod]
        public void TesGetUserIdByEmptyEmployeeId()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            try {
                string id = service.GetUserId("");
                Assert.Fail("GetUserId method should throw and exception Employee ID is empty string");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Employee Id is required", ex.Message);
            }
        }

        [TestMethod]
        public void TesGetUserIWithNullEmployeeId()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            try
            {
                string id = service.GetUserId(null);
                Assert.Fail("GetUserId method should throw and exception Employee ID is null");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Employee Id is required", ex.Message);
            }
        }

        [TestMethod]
        public void TestCreateActivity()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);

            DateTime dt = new DateTime(2011, 7, 6, 10, 11, 12);
            service.CreateActivity(_userId, "Test Activity from ChatterServiceTest.TestCreateActivity:" + dt, dt);
        }

        [TestMethod]
        public void TestCreateActivityUsingApex()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);

            DateTime dt = new DateTime(2011, 7, 6, 10, 11, 12);
            service.CreateActivityUsingApex(_userId, "Test Activity from ChatterServiceTest.TestCreateActivityUsingApex:" + dt, dt);
        }

        [TestMethod]
        public void TestGetActivitiesByUser()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);
            List<Activity> list = service.GetActivities(_userId);
            Assert.AreEqual(10, list.Count);
        }

        [TestMethod]
        public void TestGetActivities()
        {
            IChatterService service = new ChatterService.ChatterService(_url);
            service.Login(_username, _password, _token);
            List<Activity> list = service.GetActivities();
            Assert.AreEqual(100, list.Count);
        }

    }
}
