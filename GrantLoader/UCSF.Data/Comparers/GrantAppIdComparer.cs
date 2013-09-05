using System.Collections.Generic;

namespace UCSF.Data.Comparers
{
    public class GrantAppIdComparer : IEqualityComparer<Grant>
    {
        public bool Equals(Grant x, Grant y)
        {
            return x.ApplicationId == y.ApplicationId;
        }

        public int GetHashCode(Grant obj)
        {
            return obj.GetHashCode();
        }
    }
}