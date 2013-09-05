using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UCSF.Data;
using UCSF.Data.Comparers;
using UCSF.Framework.Utils;
using log4net;

namespace UCSF.Business.Web
{
    public class GrantOnlineValidator
    {
        private ILog log = LogManager.GetLogger(typeof (GrantOnlineValidator));

        private UCSDDataContext ucsdDataContext;

        private int totalProcessed;
        public int TotalProcessed
        {
            get { return totalProcessed; }
            private set { totalProcessed = value; }
        }

        private int invalidGrants;
        public int InvalidGrants
        {
            get { return invalidGrants; }
            private set { invalidGrants = value; }
        }

        private int errorsCount;
        public int ErrorsCount
        {
            get { return errorsCount; }
            private set { errorsCount = value; }
        }

        public string ValidationUrl { get; private set; }
        public string ValidationPattern { get; private set; }
        protected string RequiredKey { get; private set; }

        public void ValidateGrants()
        {
            GetSettings();

            ucsdDataContext = new UCSDDataContext();
            var grants = ucsdDataContext.Grants.Where(g => g.IsVerified == null && g.GrantPrincipals.Any(
                        gp => gp.PrincipalInvestigator != null && gp.PrincipalInvestigator.EmployeeId != null));

            string pause = ConfigurationManager.AppSettings["GrantValidation.Pause"];
            int _pause = 10;
            if (!String.IsNullOrWhiteSpace(pause))
            {
                _pause = Int32.Parse(pause);
            }

            foreach (Grant grant in grants.ToList().Distinct(new GrantAppIdComparer()))
            {
               ValidateGrantOnline(grant);
               
                if(_pause > 0)
                {
                    log.Info("Pause");
                    Thread.Sleep(_pause * 1000);
                }
            }
        }

        private void GetSettings()
        {
            ValidationUrl = ConfigurationManager.AppSettings["GrantValidation.Url"];
            ValidationPattern = ConfigurationManager.AppSettings["GrantValidation.Pattern"];
            RequiredKey = ConfigurationManager.AppSettings["GrantValidation.RequiredKey"];
        }

        private void ValidateGrantOnline(Grant grant)
        {
            Interlocked.Increment(ref totalProcessed);

            try
            {
                WebClientEx wc = new WebClientEx();
                wc.Headers.Add(HttpRequestHeader.UserAgent, ConfigurationManager.AppSettings["GrantValidation.Header"]);

                using (MemoryStream ms = new MemoryStream(wc.DownloadData(string.Format(ValidationUrl, grant.ApplicationId))))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    StreamReader sb = new StreamReader(ms);
                    string doc = sb.ReadToEnd();

                    if(doc.IndexOf(RequiredKey) <0)
                    {
                        throw new Exception("Server returned wrong response");
                    }

                    grant.IsVerified = doc.IndexOf(ValidationPattern) < 0;
                    
                    ucsdDataContext.SubmitChanges();

                    if (grant.IsVerified == true)
                    {
                        log.InfoFormat("Grant ApplicationId {0} Validated", grant.ApplicationId);
                    }
                    else
                    {
                        Interlocked.Increment(ref invalidGrants);
                        log.InfoFormat("Grant ApplicationId {0} Is not valid", grant.ApplicationId);
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(String.Format("Grant validation error: {0}", ex.Message));
                log.Debug("Error", ex);

                Interlocked.Increment(ref errorsCount);
            }
         }
    }
};