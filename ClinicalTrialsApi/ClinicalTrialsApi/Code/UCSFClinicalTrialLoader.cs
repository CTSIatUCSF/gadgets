using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ClinicalTrialsApi
{
    public class UCSFClinicalTrialLoader
    {
        private const string DATE_FORMAT = "yyyy-M-d";

        public Dictionary<string, Models.ClinicalTrial> ClinicalTrials{ get; set; }
        public Dictionary<string, IList<Models.ClinicalTrial>> PersonClinicalTrials { get; set; }

        public UCSFClinicalTrialLoader() {
            ClinicalTrials = new Dictionary<string, Models.ClinicalTrial>();
            PersonClinicalTrials = new Dictionary<string, IList<Models.ClinicalTrial>>();
        }

        public void Load(string fileName) {
            var serializer = new JsonSerializer();
            using (StreamReader sr = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {                
                dynamic trialList = serializer.Deserialize<ExpandoObject>(reader);
                foreach (dynamic trial in trialList)
                {
                    StringBuilder conditions = new StringBuilder();
                    foreach (dynamic condition in trial.Value.conditions) {
                        if (conditions.Length > 0) {
                            conditions.Append(",");
                        }
                        conditions.Append(condition.name);
                    }

                    var clinicalTrial = new Models.ClinicalTrial { 
                        Id = trial.Value.id,
                        Title = trial.Value.title_brief,
                        StartDate = DateTime.ParseExact(trial.Value.start_date, DATE_FORMAT, CultureInfo.InvariantCulture),
                        Status = trial.Value.recruitment_status,//TODO: need to use correct property
                        Conditions =  conditions.ToString()  
                    };
                    ClinicalTrials.Add(clinicalTrial.Id, clinicalTrial);

                    foreach (var pis in trial.Value.institutional_pis) {
                        var uri = pis.profile_url;
                        IList<Models.ClinicalTrial> trials = null;
                        if (!PersonClinicalTrials.TryGetValue(uri, out trials))
                        {
                            trials = new List<Models.ClinicalTrial>();
                            PersonClinicalTrials.Add(uri, trials);
                        }

                        trials.Add(clinicalTrial);
                    }

                }
            }
        }
    }
}