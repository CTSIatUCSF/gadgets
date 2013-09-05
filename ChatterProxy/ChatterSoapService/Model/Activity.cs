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
        LinkPost,
        ContentPost
    }

    [DataContract]
    public class Activity
    {
        internal Activity()
        {
        }

        /// <summary>
        /// Generated unique ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Who created the activity
        /// </summary>
        public string CreatedById { get; set; }

        public Entity Parent { get; set; }

        /// <summary>
        /// Simple TimeStamp 
        /// </summary>
        public DateTime CreatedDT { get; set; }

        [DataMember(Name = "d")]
        public string Date
        {
            get { return String.Format("{0:MMMM d, yyyy}", CreatedDT); }
            set { }
        }

        [DataMember(Name="m")]
        public string Message { get; set; }

        [DataMember(Name = "l")]
        public string LinkUrl { get; set; }

        [DataMember(Name = "t")]
        public string Title { get; set; }

        public ActivityType Type { get; set; }

        [DataMember(Name="n")]
        public string ParentName
        {
            get { return Parent.Name; }
            set {}
        }
        [DataMember(Name = "id")]
        public int ParentId
        {
            get { return Parent.GetType() == typeof(User) ? ((User)Parent).PersonId : 0; }
            set { }
        }
    }

    public class ActivitiesComparer : IComparer<Activity>
    {
        #region IComparer<Activity> Members

        public int Compare(Activity x, Activity y)
        {
            return DateTime.Compare(y.CreatedDT, x.CreatedDT);
        }

        #endregion
    }

}
