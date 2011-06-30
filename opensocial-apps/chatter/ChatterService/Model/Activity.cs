using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatterService.Model
{
    public class Activity
    {
        /// <summary>
        /// Generated unique ID
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Who created the activity
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID of the gadget that created the Activity.  NULL for non-gadget created activities (such as those created by DB batch jobs)
        /// </summary>
        public int? AppId { get; set; }

        /// <summary>
        /// Simple TimeStamp 
        /// </summary>
        public TimeSpan CreatedDT { get; set; }

        /// <summary>
        /// XML blob
        /// </summary>
        public string Message { get; set; }
    }
}
