using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatterService.Model
{
    class Group: Entity
    {
        public string Description { get; set; }
        public User Owner { get; set; }        
    }
}
