using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChatterService.Model
{
    [DataContract(IsReference = false)]
    [KnownType(typeof(User))]
    public class Entity
    {
        internal Entity()
        {
        }

        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
