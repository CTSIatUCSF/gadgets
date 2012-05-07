using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UCSF.Data;
using UCSF.Data.Comparers;
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

        public void ValidateGrants()
        {
            ucsdDataContext = new UCSDDataContext();
            var grants = ucsdDataContext.Grants.Where(g => g.IsVerified == null && g.GrantPrincipals.Any(
                        gp => gp.PrincipalInvestigator != null && gp.PrincipalInvestigator.EmployeeId != null));

            foreach (Grant grant in grants.ToList().Distinct(new GrantAppIdComparer()))
            {
               ValidateGrantOnline(grant);
            }
        }

        private void ValidateGrantOnline(Grant grant)
        {
            Interlocked.Increment(ref totalProcessed);

            try
            {
                WebClient wc = new WebClient();
                using (MemoryStream ms = new MemoryStream(wc.DownloadData(string.Format("http://projectreporter.nih.gov/project_info_description.cfm?aid={0}",
                                              grant.ApplicationId))))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    StreamReader sb = new StreamReader(ms);
                    string doc = sb.ReadToEnd();

                    grant.IsVerified = doc.IndexOf("This project doesn't exist in RePORTER") < 0;
                    
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
                log.Error("Grant validation error", ex);

                Interlocked.Increment(ref errorsCount);
            }
         }
    }
};