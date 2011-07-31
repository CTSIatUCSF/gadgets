using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChatterService.Model
{
    [DataContract(IsReference = false)]
    class User : Entity
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string EmployeeId { get; set; }

        [DataMember]
        public int PersonId { get; set; }
    }
}
