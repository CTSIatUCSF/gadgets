using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using UCSF.Data;
using UCSF.Framework.Utils;
using log4net;

namespace UCSF.Business.DataImporter
{
    public class GrantImporter : GrantImporterBase
    {
        private TransactionScope ts;

        protected override void StartTransaction()
        {
            ts = new TransactionScope();
        }

        protected override void CompleteTransaction()
        {
            try
            {
                DataContext.SubmitChanges();
                ts.Complete();
            }
            catch (Exception ex)
            {
                log.Info("Error saving data to server");
                log.Debug("Error saving data to server", ex);
            }
            finally
            {
                ts.Dispose();
            }
        }

        protected override bool CheckIfGrantExists(Grant grant)
        {
            return DataContext.Grants.Any(it => it.ApplicationId == grant.ApplicationId);
        }

        protected override PrincipalInvestigator GetPrincipalInvestigator(int principalInvestigatorId)
        {
            return DataContext.PrincipalInvestigators.FirstOrDefault(it => it.PrincipalInvestigatorId == principalInvestigatorId) ??
                   DataContext.GetChangeSet().Inserts.FirstOrDefault(it => it is PrincipalInvestigator && (it as PrincipalInvestigator).PrincipalInvestigatorId == principalInvestigatorId) as PrincipalInvestigator;
        }
    }
}