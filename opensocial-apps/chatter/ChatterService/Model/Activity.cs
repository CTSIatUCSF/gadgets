using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChatterService.Model
{
    [DataContract]
    public enum ActivityType
    {
        TrackedChanges,
        UserStatus,
        TextPost,
        ContentPost
    }

    [DataContract]
    public class Activity
    {
        /// <summary>
        /// Generated unique ID
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Who created the activity
        /// </summary>
        [DataMember]
        public string CreatedById { get; set; }

        [DataMember]
        public Entity Parent { get; set; }

        /// <summary>
        /// Simple TimeStamp 
        /// </summary>
        [DataMember]
        public DateTime? CreatedDT { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public ActivityType Type { get; set; }
    }
}
