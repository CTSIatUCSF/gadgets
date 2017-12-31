using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicalTrialsApi.Models
{
    public class ClinicalTrial
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime StartDate { get; set; }

        public string Conditions { get; set; }

        public string Status { get; set; }

        public bool IsMultiSite { get; set; }
    }
}