using System;
using System.Diagnostics;
using System.Web.Http;
using ClinicalTrialsApi.App_Start;
using System.Web;
using System.IO;
using System.Collections.Generic;

namespace ClinicalTrialsApi
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Configure);

            string dirName = HttpContext.Current.Server.MapPath(@"~/Content/");
            string[] files = Directory.GetFiles(dirName, "*.json");

            List<string> domainNames = new List<string>();
            foreach (string fileName in files) {
                String domainName = Path.GetFileNameWithoutExtension(fileName);
                domainNames.Add(domainName);

                var loader = new UCSFClinicalTrialLoader();
                loader.Load(fileName, domainName);


                HttpRuntime.Cache.Insert("ClinicalTrials:" + domainName, loader.ClinicalTrials);
                HttpRuntime.Cache.Insert("PersonClinicalTrials:" + domainName, loader.PersonClinicalTrials);
            }
            HttpRuntime.Cache.Insert("DomainNames", domainNames);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string dirName = HttpContext.Current.Server.MapPath(@"~/Content/");
            string[] files = Directory.GetFiles(dirName, "*.json");
            foreach (string fileName in files)
            {
                String domainName = Path.GetFileNameWithoutExtension(fileName);
                break;
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}