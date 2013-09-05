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
using System.Configuration;

namespace ChatterServiceTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ChatterSoapServiceTest
    {
        private static bool customXertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public ChatterSoapServiceTest()
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
        string _url = ConfigurationSettings.AppSettings["SalesForceUrl"];
        string _username = ConfigurationSettings.AppSettings["SalesForceUserName"];
        string _password = ConfigurationSettings.AppSettings["SalesForcePassword"];
        string _token = ConfigurationSettings.AppSettings["SalesForceToken"];

            string _employeeId = "025693078";
            string _userId = "005A0000001X3NbIAK";
            int _personId = 5138614;
        

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
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
        }

        [TestMethod]
        public void TestGetUserId()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);

            string id = service.GetUserId(_employeeId);
            Assert.AreEqual(_userId, id);            
        }

        [TestMethod]
        public void TestGetUserIdByEmptyEmployeeId()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
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
        public void TestGetUserIWithNullEmployeeId()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
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
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);

            DateTime dt = new DateTime(2011, 7, 6, 10, 11, 12);
            service.CreateActivity(_userId, null, "Edited their narrative", "Test Activity from ChatterServiceTest.TestCreateActivity:" + dt, dt);
        }

        [TestMethod]
        public void TestCreateActivityUsingApex()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);

            DateTime dt = DateTime.Now;
            service.CreateActivityUsingApex(_userId, null, "Edited their narrative", "Test Activity from 'ChatterServiceTest.TestCreateActivityUsingApex':" + dt, dt);
        }

        [TestMethod]
        public void TestGetActivitiesByUser()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            List<Activity> list = service.GetUserActivities(_userId, 123, 10);
            Assert.AreEqual(10, list.Count);
        }


        [TestMethod]
        public void TestCreateReseachProfile()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            service.CreateResearchProfile(_employeeId);
        }

        [TestMethod]
        public void TestCreateProfileActivity()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);

            //DateTime dt = new DateTime(2012, 7, 29, 14, 11, 12);
            DateTime dt = DateTime.Now;
            service.CreateProfileActivity("025693078", null, "Edited their narrative", "Test Activity from 'TestCreateReseachProfile':" + dt, dt);
            //service.CreateProfileActivity("025693078", "Null body test", null, dt);
            //service.CreateProfileActivity(_employeeId, "Edited their narrative", "Test Activity from 'TestCreateReseachProfile':" + dt, dt);
            
        }

        [TestMethod]
        public void TestCreateExternalMessage()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            service.CreateExternalMessage("http://orng.info", "Test", "Test of create external message", "025693078");
        }

        [TestMethod]
        public void TestGetPersonId()
        {
            int id = ChatterService.ChatterSoapService.GetPersonId("025693078");
            Assert.AreEqual(5138614, id);
        }

        [TestMethod]
        public void TestCreateGroup()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            service.CreateGroup("aa1", "This group was created by unit test", _employeeId);
            service.CreateGroup("aa2", "This group was created by\r\nunit test", _employeeId);
            byte r = 0x000D;
            byte n = 0x000A & 0x000A;
            String two ="hello" + Convert.ToString(r) + "world" + r + "odd" + r + "" + n + "very" + r + n + "test";
            String three ="hello" + Convert.ToString(r) + Convert.ToString(n) + "world";
            service.CreateGroup("aa3", two, _employeeId);
            service.CreateGroup("aa4", three, _employeeId);
        }

        [TestMethod]
        public void TestUsersToGroup()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            string id = service.CreateGroup("Test API Group 3", "This group was created by unit test", _employeeId);

            service.AddUsersToGroup(id, new string[] { "020524930", "027639251" });
        }
        

        [TestMethod]
        public void TestGetGroup()
        {
            IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.Login(_username, _password, _token);
            string id = "0F9Z000000007Zu";

            Salesforce.CollaborationGroup group = service.GetCollaborationGroup(id);
        }

        [TestMethod]
        public void TestDeleteActivitiesFromDisabledProfiles()
        {
            List<int> peopleToDelete = new ProfilesServices().GetInactiveProfiles();
            System.Diagnostics.Debug.WriteLine("Found " + peopleToDelete.Count + " people to remove from activity stream");
            int total = 0;
            List<Activity> killList = null;
            do
            {
                killList = new List<Activity>();
                try
                {
                    IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
                    service.Login(_username, _password, _token);
                    // just do 1000 at a time
                    foreach (Activity act in service.GetProfileActivities((Activity)null, 200))
                    {
                        if (peopleToDelete.Contains(act.ParentId))
                        {
                            killList.Add(act);
                            System.Diagnostics.Debug.WriteLine("Deleting " + act.Id + " : " + act.Message + " for " + act.ParentName);
                        }
                    }
                    ((ChatterSoapService)service).DeleteActivities(killList);
                    System.Diagnostics.Debug.WriteLine("Deleted " + killList.Count + " activities");
                    total += killList.Count;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            } while (killList.Count > 0);
            System.Diagnostics.Debug.WriteLine("Total removed = " + total);
        }
    }
}
