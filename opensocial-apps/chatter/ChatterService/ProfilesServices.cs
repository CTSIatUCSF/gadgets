using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using ChatterService.Dao;

namespace ChatterService
{
    public class ProfilesServices : IProfilesServices
    {
        #region IProfilesServices Members

        public string GetEmployeeId(int personId)
        {
            ProfilesDataContext dc = new ProfilesDataContext();
            string employeeId = (from p in dc.GetTable<person>() where (p.PersonID == personId) select p.InternalUsername).FirstOrDefault();

            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Person not found, personId=" + personId);
            }

            return employeeId;
        }

        #endregion
    }
}
