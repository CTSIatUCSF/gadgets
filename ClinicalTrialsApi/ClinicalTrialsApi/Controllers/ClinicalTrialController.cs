using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using System.Text;

namespace ClinicalTrialsApi.Controllers
{
    public class ClinicalTrialController : ApiController
    {
        private static HttpClient httpClient = new HttpClient();

        [HttpGet]
        public IHttpActionResult Index([FromUri] string profile_uri) {
            Dictionary<string, IList<Models.ClinicalTrial>> clinicalTrials = (Dictionary<string, IList<Models.ClinicalTrial>>)HttpRuntime.Cache.Get("PersonClinicalTrials");

            IList<Models.ClinicalTrial> trials = null;
            if (clinicalTrials.TryGetValue(profile_uri, out trials)) {
                return Ok(trials);
            }
            return NotFound();
        }

        [HttpGet]
        public IHttpActionResult Index([FromUri] string[] ids)
        {
            List<Models.ClinicalTrial> result = new List<Models.ClinicalTrial>();
            Dictionary<string, Models.ClinicalTrial> clinicalTrials = (Dictionary<string, Models.ClinicalTrial>)HttpRuntime.Cache.Get("ClinicalTrials");
            foreach(var id in ids) {
                var clinicalTrial = getTrial(clinicalTrials, id);
                result.Add(clinicalTrial);
            }
            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            Dictionary<string, Models.ClinicalTrial> clinicalTrials = (Dictionary<string, Models.ClinicalTrial>)HttpRuntime.Cache.Get("ClinicalTrials");
            Models.ClinicalTrial clinicalTrial = getTrial(clinicalTrials, id);

            return Ok(clinicalTrial);
        }

        private Models.ClinicalTrial getTrial(Dictionary<string, Models.ClinicalTrial> clinicalTrials, string id) {
            Models.ClinicalTrial clinicalTrial = null;
            if (clinicalTrials.TryGetValue(id, out clinicalTrial))
            {
                return clinicalTrial;
            }

            var xml = Task.Run(() => LoadTrial(id)).GetAwaiter().GetResult();

            return BuildClinicalTrial(id, xml);
        }

        private async Task<string> LoadTrial(string id) {
            using (var response = await httpClient.GetAsync("https://clinicaltrials.gov/ct2/show/" + id + "?displayxml=true"))
            {
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
                Status = doc.DocumentElement.SelectSingleNode("overall_status").InnerText,
                Conditions = conditions.ToString()
            };
        }
    }
}