using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Text;

namespace ClinicalTrialsApi.Controllers
{
    public class ClinicalTrialController : ApiController
    {
        private static HttpClient httpClient = new HttpClient();

        [HttpGet]
        public IHttpActionResult Index([FromUri] string person_url) {
            string domainName = getDomainName(person_url);
            if (domainName == null) {
                return NotFound();
            }

            Dictionary<string, IList<Models.ClinicalTrial>> clinicalTrials = (Dictionary<string, IList<Models.ClinicalTrial>>)HttpRuntime.Cache.Get("PersonClinicalTrials:" + domainName);

            IList<Models.ClinicalTrial> trials = null;
            if (clinicalTrials.TryGetValue(person_url, out trials)) {
                return Ok(trials.OrderByDescending(t => t.StartDate).ToArray());
            }
            return NotFound();
        }

        [HttpGet]
        public IHttpActionResult Index([FromUri] string[] ids, [FromUri] string profile_url)
        {
            string domainName = getDomainName(profile_url);
            if (domainName == null)
            {
                return NotFound();
            }

            List<Models.ClinicalTrial> result = new List<Models.ClinicalTrial>();
            Dictionary<string, Models.ClinicalTrial> clinicalTrials = (Dictionary<string, Models.ClinicalTrial>)HttpRuntime.Cache.Get("ClinicalTrials:" + domainName);
            foreach (var trialId in ids[0].Split(',')) {
                var clinicalTrial = getTrial(clinicalTrials, trialId);
                if (clinicalTrial != null) {
                    result.Add(clinicalTrial);
                }
            }

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult Get(string id, [FromUri] string profile_url)
        {
            string domainName = getDomainName(profile_url);
            if (domainName == null)
            {
                return NotFound();
            }

            Dictionary<string, Models.ClinicalTrial> clinicalTrials = (Dictionary<string, Models.ClinicalTrial>)HttpRuntime.Cache.Get("ClinicalTrials:" + domainName);
            Models.ClinicalTrial clinicalTrial = getTrial(clinicalTrials, id);
            if (clinicalTrial == null) {
                return NotFound();
            }

            return Ok(clinicalTrial);
        }

        private Models.ClinicalTrial getTrial(Dictionary<string, Models.ClinicalTrial> clinicalTrials, string id) {
            Models.ClinicalTrial clinicalTrial = null;
            if (clinicalTrials.TryGetValue(id, out clinicalTrial))
            {
                return clinicalTrial;
            }

            var xml = Task.Run(() => LoadTrial(id)).GetAwaiter().GetResult();
            if (xml == null) {
                return null;
            }

            return BuildClinicalTrial(id, xml);
        }

        private async Task<string> LoadTrial(string id) {
            using (var response = await httpClient.GetAsync("https://clinicaltrials.gov/ct2/show/" + id + "?displayxml=true"))
            {
                if (response.StatusCode == HttpStatusCode.NotFound) {
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var xmlResponse = await response.Content.ReadAsStringAsync();
                return xmlResponse;
            }
        }

        private Models.ClinicalTrial BuildClinicalTrial(string id, string xml) {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var title = doc.DocumentElement.SelectSingleNode("brief_title").InnerText;

            StringBuilder conditions = new StringBuilder();
            foreach (XmlNode condition in doc.DocumentElement.SelectNodes("condition")) {
                if (conditions.Length > 0)
                {
                    conditions.Append(",");
                }
                conditions.Append(condition.InnerText);
            }

            return new Models.ClinicalTrial
            {
                Id = id,
                Title = doc.DocumentElement.SelectSingleNode("brief_title").InnerText,
                StartDate = DateTime.Parse(doc.DocumentElement.SelectSingleNode("start_date").InnerText),
                CompletionDate = DateTime.Parse(doc.DocumentElement.SelectSingleNode("completion_date").InnerText),
                Status = doc.DocumentElement.SelectSingleNode("overall_status").InnerText,
                Conditions = conditions.ToString(),
                SourceUrl = "https://clinicaltrials.gov/ct2/show/" + id
            };
        }

        private string getDomainName(string uri) {
            List<string> domainNames = (List<string>)HttpRuntime.Cache.Get("DomainNames");
            foreach (string domainName in domainNames) {
                if (uri.ToLower().Contains(domainName)) {
                    return domainName;
                }
            }
            return null;
        }
    }
}