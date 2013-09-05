using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using UCSF.Data;

namespace UCSF.Business.DataImporter
{
    public class BulkImporter : GrantImporterBase
    {
        List<Grant> grants = new List<Grant>();
        List<PrincipalInvestigator> pis = new List<PrincipalInvestigator>();
        List<GrantPrincipal> grantPis = new List<GrantPrincipal>();

        public BulkImporter()
        {
            RecordsPerTransaction = 100;
        }

        protected override void AddGrantToRecordset(Grant grant)
        {
            grants.Add(grant);
            pis.AddRange(grant.GrantPrincipals.Where(it => it.PrincipalInvestigator != null).Select(p => p.PrincipalInvestigator));
            grantPis.AddRange(grant.GrantPrincipals);
        }

        protected override bool CheckIfGrantExists(Grant grant)
        {
            return false;
        }

        protected override void StartTransaction()
        {
            ClearLists();
        }

        protected override void CompleteTransaction()
        {
            try
            {
                DataContext.Grants.BulkInsert(grants, RecordsPerTransaction);
                DataContext.PrincipalInvestigators.BulkInsert(pis, RecordsPerTransaction);
                DataContext.GrantPrincipals.BulkInsert(grantPis, RecordsPerTransaction);
            }
            catch(Exception ex)
            {
                log.Error("Error during bulk insert");
                log.Debug("Error during bulk insert", ex);

                TotalProcessed = TotalProcessed - grants.Count;
            }
            finally
            {
                ClearLists();
            }
        }

        private void ClearLists()
        {
            grants.Clear();
            pis.Clear();
            grantPis.Clear();
        }

        protected override PrincipalInvestigator GetPrincipalInvestigator(int principalInvestigatorId)
        {
            return null;

//            return DataContext.PrincipalInvestigators.FirstOrDefault(it => it.PrincipalInvestigator_Id == principalInvestigatorId) ??
//                   pis.FirstOrDefault(it => it is PrincipalInvestigator && it.PrincipalInvestigator_Id == principalInvestigatorId);
        }
    }
}