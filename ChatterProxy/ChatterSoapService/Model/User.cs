using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChatterService.Model
{
    [DataContract(IsReference = false,
                  Namespace="",
                  Name="u")]
    public class User : Entity
    {
        internal User()
        {
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmployeeId { get; set; }

        [DataMember]
        public int PersonId { get; set; }
    }
}
