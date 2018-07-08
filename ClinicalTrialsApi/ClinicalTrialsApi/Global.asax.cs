using System;
using System.Diagnostics;
using System.Web.Http;
using ClinicalTrialsApi.App_Start;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Http;

namespace ClinicalTrialsApi
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("ClinicalTrialsApi");
        private string ContentDirectory { get; set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Configure);

            ContentDirectory = HttpContext.Current.Server.MapPath(@"~/Content/");

            Task.Run(() => UpdateContent()).GetAwaiter().GetResult();
        }

        protected string GetContentFilename(string domainName) {
            return ContentDirectory + domainName + ".json";
        }

        protected async Task UpdateContent() {
            Log.Info("Content update started...");

            var domainNames = ConfigurationManager.AppSettings["profiles.domains"].Split(',').ToList();
            HttpRuntime.Cache.Insert("DomainNames", domainNames);
            await Task.WhenAll(domainNames.Select(x => UpdateContent(x)));

            Log.Info("Content update completed.");
        }

        protected async Task UpdateContent(string domainName) {
            Log.Info($"Start content update for {domainName} domain");
            var fileName = await DownloadContentFile(domainName);

            var loader = new UCSFClinicalTrialLoader();
            loader.Load(fileName, domainName);

            HttpRuntime.Cache.Insert("ClinicalTrials:" + domainName, loader.ClinicalTrials);
            HttpRuntime.Cache.Insert("PersonClinicalTrials:" + domainName, loader.PersonClinicalTrials);

            Log.Info($"Completed content update for {domainName} domain");
        }

        protected async Task<string> DownloadContentFile(string domainName) {
            var fileName = GetContentFilename(domainName) + ".gz";

            using (var response = await new HttpClient().GetAsync("https://clinicaltrials." + domainName + "/__data_export.json.gz"))
            using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.SetLength(0);
                await response.Content.CopyToAsync(fileStream);
            }

            return Decompress(fileName);
        }

        protected string Decompress(string fileName)
        {
            var fileToDecompress = new FileInfo(fileName);
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }

                return newFileName;
            }
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