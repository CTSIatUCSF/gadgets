using System;
using System.Diagnostics;
using System.Web.Http;
using ClinicalTrialsApi.App_Start;
using System.Web;

namespace ClinicalTrialsApi
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Configure);

            string fileName = HttpContext.Current.Server.MapPath(@"~/Content/__data_export.json");
            var loader = new UCSFClinicalTrialLoader();
            loader.Load(fileName);

            HttpRuntime.Cache.Insert("ClinicalTrials", loader.ClinicalTrials);
            HttpRuntime.Cache.Insert("PersonClinicalTrials", loader.PersonClinicalTrials);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

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