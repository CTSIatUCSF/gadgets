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
using System.Security.Cryptography;
using DevDefined.OAuth;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Framework.Signing;

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
        string _url = ConfigurationSettings.AppSettings["SalesForceUrl"];
        string _client_id = ConfigurationSettings.AppSettings["SalesForceClientId"];
        string _grant_type = ConfigurationSettings.AppSettings["SalesForceGrantType"];
        string _client_secret = ConfigurationSettings.AppSettings["SalesForceClientSecret"];
        string _username = ConfigurationSettings.AppSettings["SalesForceUserName"];
        string _password = ConfigurationSettings.AppSettings["SalesForcePassword"];
        bool _logService = Boolean.Parse(ConfigurationSettings.AppSettings["LogService"]);
        

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
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url, _logService);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
        }

        [TestMethod]
        public void TestRestGetFollowers()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url, _logService);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.GetFollowers("005A0000001X3Nb");
        }

        [TestMethod]
        public void TestRestGetFollowing()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url, _logService);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.GetFollowing("005A0000001X3Nb");
        }

        [TestMethod]
        public void TestRestFollow()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url, _logService);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.Follow("005A0000001X3Nb", "005A0000001X2Yp");
        }

        [TestMethod]
        public void TestRestUnfollow()
        {
            ChatterService.ChatterRestService service = new ChatterService.ChatterRestService(_url, _logService);
            service.Login(_client_id, _grant_type, _client_secret, _username, _password);
            service.Unfollow("005A0000001X3Nb", "005A0000001X3gUIAS");
        }

        [TestMethod]
        public void TestOAuth()
        {
            X509Certificate2 cert = new X509Certificate2(ConfigurationSettings.AppSettings["OAuthCert"]);
            AsymmetricAlgorithm provider = cert.PublicKey.Key;
            OAuthContextSigner signer = new OAuthContextSigner();
            SigningContext signingContext = new SigningContext();
            //signingContext.ConsumerSecret = ...; // if there is a consumer secret
            signingContext.Algorithm = provider;

            Uri uri = new Uri(
                "http://dev-profiles.campus.net.ucsf.edu/chatter/ChatterProxyService.svc/user/5138614/unfollow/4621800?accessToken=00DZ0000000jhLQ!ARIAQAlqX_qtYj95uzEftkMIKQggfo.RoJ3KnvvakO97Xrjptfq89vTtwGFgR1jnyeNSm1CwnLSSz0N3g8.bQrX.jCpJ6Np3&oauth_body_hash=2jmj7l5rSw0yVb/vlWAYkK/YBwk=&opensocial_owner_id=4621800&opensocial_viewer_id=5138614&opensocial_app_id=http://dev-profiles.ucsf.edu/ORNG/ChatterFollow.xml&opensocial_app_url=http://dev-profiles.ucsf.edu/ORNG/ChatterFollow.xml&oauth_consumer_key=&xoauth_signature_publickey=mytestkey&xoauth_public_key=mytestkey&oauth_version=1.0&oauth_timestamp=1349466703&oauth_nonce=7533897618501371565&oauth_consumer_key=&oauth_signature_method=RSA-SHA1&oauth_signature=d0UIIXK+HwbkLD4VE59ylZ9XoBreMBqc0Kcf4v2DjzWT0AE1JtCUhDmS1Uy1P9K54tpeoQwjcu8mnWsA7PQpTRTYyU1k+ueT4M2ihoaB+CunpZz6Q3KE8MUZn4Sy0D7iNuje6WdgHZ80f9Ln8OwRPzrfHA5v0KowATRv7T2h+x0="
                );

            IOAuthContext context = new OAuthContextBuilder().FromUri("GET", uri);

            // use context.ConsumerKey to fetch information required for signature validation for this consumer.
            if (!signer.ValidateSignature(context, signingContext))
            {
                throw new Exception("Invalid signature : " + uri);
            }
        }
    }
}
