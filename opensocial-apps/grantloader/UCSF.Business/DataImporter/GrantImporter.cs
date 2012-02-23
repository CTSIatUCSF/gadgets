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
    public class GrantImporter
    {
        private const string ORG_NAME = "ORG_NAME";
        public UCSDDataContext DataContext { get; private set; }

        private StreamWriter errorsStream;
        private StreamWriter successStream;

        private static ILog log;

        public GrantImporter(): this(new UCSDDataContext())
        {
        }

        public GrantImporter(UCSDDataContext dataContext)
        {
            log = LogManager.GetLogger(GetType());
            DataContext = dataContext;
        }

        public int ErrorsCount { get; private set; }

        public int TotalRecords{ get; private set; }
        
        public int TotalProcessed{ get; private set; }

        private string orgName;
        private bool checkOrgName;

        public void ImportData(string uri, string orgName = null, string DUNSNumber = null)
        {
            if(!File.Exists(uri))
            {
                return;
            }

            this.orgName = orgName;
            checkOrgName = !String.IsNullOrWhiteSpace(orgName);

            errorsStream = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors.txt"));
            errorsStream.AutoFlush = true;
            successStream = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "processed.txt"));
            successStream.AutoFlush = true;

            TransactionScope ts = new TransactionScope();
            using (XmlReader reader = XmlReader.Create(uri))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "row")
                    {
                        Grant grant = new Grant { GrantId = Guid.NewGuid() };

                        XElement row = new XElement("row");
                        bool needContinue = true;
                        try
                        {
                            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                            {
                                if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                                {
                                    XElement node = XNode.ReadFrom(reader) as XElement;
                                    row.Add(node);
                                    if (!String.IsNullOrWhiteSpace(node.Value))
                                    {
                                        needContinue = UpdateGrant(grant, node);
                                        
                                        if(!needContinue)
                                            break;
                                    }
                                }
                            }

                            TotalProcessed++;

                            if (!needContinue || (checkOrgName && !String.Equals(grant.OrgName, this.orgName, StringComparison.OrdinalIgnoreCase)))
                                continue;

                            grant.XML = row.ToString();

                            #region check if need to process the record
                            if (DataContext.Grants.Any(it => it.ApplicationId == grant.ApplicationId))
                            {
                                continue;
                            }
                            #endregion
                            ValidateGrant(grant);
                            DataContext.Grants.InsertOnSubmit(grant);
                            AddSuccessGrunt(grant);

                            if (TotalRecords % 100 == 0)
                            {
                                Submit(ts);

                                DataContext = new UCSDDataContext();

                                ts = new TransactionScope();

                                log.InfoFormat("Processed {0} rows.", TotalRecords);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.InfoFormat("Error processing row with ApplicationID = {0}. {1}", grant.ApplicationId, ex.Message);
                            log.Debug(String.Format("Error processing row with ApplicationID = {0}\r\n{1}", grant.ApplicationId, ex.Message), ex);

                            AddGrantError(grant, ex);
                        }
                    }
                }
            }

            FlushStream(errorsStream);
            FlushStream(successStream);
            
            Submit(ts);
        }

        private void FlushStream(StreamWriter stream)
        {
            try
            {
                stream.Flush();
            }
            catch(Exception ex)
            {
                log.Debug("Error closing file.", ex);
            }
            finally
            {
                stream.Close();
            }
        }

        private void Submit(TransactionScope ts)
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

        #region grant validation
        private void ValidateGrant(Grant grant)
        {
            ValidateField(grant.Activity, 50);
            ValidateField(grant.AdministeringIC, 50);
            ValidateField(grant.FOANumber, 255);
            ValidateField(grant.FullProjectNum, 255);
            ValidateField(grant.FundingICS, 400);
            ValidateField(grant.OrgCity, 255);
            ValidateField(grant.OrgCountry, 255);
            ValidateField(grant.OrgDistrict, 255);
            ValidateField(grant.OrgDept, 255);
            ValidateField(grant.OrgFIPS, 255);
            ValidateField(grant.OrgState, 2);
            ValidateField(grant.OrgZip, 9);
            ValidateField(grant.ICName, 255);
            ValidateField(grant.OrgName, 255);
            ValidateField(grant.ProjectTitle, 255);
            ValidateField(grant.CoreProjectNumber, 50);
        }

        private void ValidateField(string data, int length)
        {
            if(!String.IsNullOrEmpty(data) && data.Length > length)
            {
                throw new Exception(String.Format("String exceed maximum length of {0} chars.", length));
            }
        }

        #endregion

        private void AddSuccessGrunt(Grant grant)
        {
            TotalRecords++;
            successStream.WriteLine(String.Format("{0},{1}", TotalRecords, grant.ApplicationId));
        }

        private void AddGrantError(Grant grant, Exception exception)
        {
            ErrorsCount++;
            errorsStream.WriteLine(String.Format("{0},{1},{2}", ErrorsCount, grant.ApplicationId, exception.Message));
        }

        private bool UpdateGrant(Grant grant, XElement node)
        {
            switch (node.Name.LocalName)
            {
                case "APPLICATION_ID":
                    grant.ApplicationId = Convert.ToInt32(node.Value);
                    break;
                case "ACTIVITY":
                    grant.Activity = node.Value.SafeTrim();
                    break;
                case "ADMINISTERING_IC":
                    grant.AdministeringIC = node.Value.SafeTrim();
                    break;
                case "APPLICATION_TYPE":
                    grant.ApplicationType = Convert.ToInt32(node.Value);
                    break;
                case "ARRA_FUNDED":
                    grant.ARRAFunded = node.Value == "Y";
                    break;
                case "BUDGET_START":
                    grant.BudgetStart = Convert.ToDateTime(node.Value);
                    break;
                case "BUDGET_END":
                    grant.BudgetEnd = Convert.ToDateTime(node.Value);
                    break;
                case "FOA_NUMBER":
                    grant.FOANumber = node.Value.SafeTrim();
                    break;
                case "FULL_PROJECT_NUM":
                    grant.FullProjectNum = node.Value.SafeTrim();
                    break;
                case "FUNDING_ICs":
                    grant.FundingICS = node.Value.SafeTrim();
                    break;
                case "FY":
                    grant.FY = Convert.ToInt32(node.Value);
                    break;
                case "ORG_CITY":
                    grant.OrgCity = node.Value.SafeTrim();
                    break;
                case "ORG_COUNTRY":
                    grant.OrgCountry = node.Value.SafeTrim();
                    break;
                case "ORG_DISTRICT":
                    grant.OrgDistrict = node.Value.SafeTrim();
                    break;
                case "ORG_DUNS":
                    grant.OrgDUNS = Convert.ToInt32(node.Value);
                    break;
                case "ORG_DEPT":
                    grant.OrgDept = node.Value.SafeTrim();
                    break;
                case "ORG_STATE":
                    grant.OrgState = node.Value.SafeTrim();
                    break;
                case "ORG_ZIPCODE":
                    grant.OrgZip = node.Value.SafeTrim();
                    break;
                case "IC_NAME":
                    grant.ICName = node.Value.SafeTrim();
                    break;
                case ORG_NAME:
                    grant.OrgName = node.Value.SafeTrim();

                    if (checkOrgName && !String.Equals(grant.OrgName, this.orgName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    break;
                case "ORG_FIPS":
                    grant.OrgFIPS = node.Value.SafeTrim();
                    break;
                case "PROJECT_TITLE":
                    grant.ProjectTitle = node.Value.SafeTrim();
                    break;
                case "PROJECT_START":
                    grant.ProjectStart = Convert.ToDateTime(node.Value);
                    break;
                case "PROJECT_END":
                    grant.ProjectEnd = Convert.ToDateTime(node.Value);
                    break;
                case "PIS":
                    UpdateGrantInvestigators(grant, node);
                    break;
                case "CORE_PROJECT_NUM":
                    grant.CoreProjectNumber= node.Value.SafeTrim();
                    break;
            }

            return true;
        }

        private void UpdateGrantInvestigators(Grant grant, XElement node)
        {
            IList<XElement> pis = node.Elements("PI").ToList();
            if (pis.Count > 0)
            {
                foreach (XElement pi in pis)
                {
                    if(String.IsNullOrWhiteSpace(pi.Element("PI_ID").Value))
                    {
                        continue;
                    }
                    int principalInvestigatorId = Convert.ToInt32(pi.Element("PI_ID").Value.Replace("(contact)", "").Trim());

                    PrincipalInvestigator investigator = DataContext.PrincipalInvestigators.FirstOrDefault(it => it.PrincipalInvestigator_Id == principalInvestigatorId) ?? 
                        DataContext.GetChangeSet().Inserts.FirstOrDefault(it => it is PrincipalInvestigator && (it as PrincipalInvestigator).PrincipalInvestigator_Id == principalInvestigatorId) as PrincipalInvestigator;

                    if(investigator == null)
                    {
                        investigator = new PrincipalInvestigator
                                           {
                                               PrincipalInvestigatorId = Guid.NewGuid(),
                                               Name = pi.Element("PI_NAME").Value.SafeTrim(),
                                               PrincipalInvestigator_Id = principalInvestigatorId
                                           };

                        DataContext.PrincipalInvestigators.InsertOnSubmit(investigator);
                    }

                    grant.GrantPrincipals.Add(new GrantPrincipal() { PrincipalInvestigator = investigator, GrantPrincipalId = Guid.NewGuid() });
                }
            }
        }
    }
}