using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ChatterService
{
    public class ProfilesServices : IProfilesServices
    {
        #region IProfilesServices Members

        public string GetEmployeeId(string nodeId)
        {
            System.Text.StringBuilder sql = new System.Text.StringBuilder();
            string employeeId = null;

            string connstr = ConfigurationManager.ConnectionStrings["ChatterService.Properties.Settings.profilesConnectionString"].ConnectionString;

            sql.AppendLine("select p.internalusername from [Profile.Data].[Person] p join [RDF.Stage].internalnodemap i on p.personid = i.internalid where i.[class] = 'http://xmlns.com/foaf/0.1/Person' and i.nodeid = " + nodeId);

            SqlConnection dbconnection = new SqlConnection(connstr);
            SqlCommand dbcommand = new SqlCommand();

            dbconnection.Open();
            dbcommand.CommandType = CommandType.Text;

            dbcommand.CommandText = sql.ToString();
            dbcommand.CommandTimeout = 5000;

            dbcommand.Connection = dbconnection;

            using (SqlDataReader dbreader = dbcommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (dbreader.Read())
                {
                    employeeId = dbreader[0].ToString();
                }
            }

            return employeeId;
        }

        public List<int> GetInactiveProfiles()
        {
            System.Text.StringBuilder sql = new System.Text.StringBuilder();

            string connstr = ConfigurationManager.ConnectionStrings["ChatterService.Properties.Settings.profilesConnectionString"].ConnectionString;

            sql.AppendLine("select personid from [Profile.Data].[Person] where IsActive = 0");

            SqlConnection dbconnection = new SqlConnection(connstr);
            SqlCommand dbcommand = new SqlCommand();

            dbconnection.Open();
            dbcommand.CommandType = CommandType.Text;

            dbcommand.CommandText = sql.ToString();
            dbcommand.CommandTimeout = 5000;

            dbcommand.Connection = dbconnection;

            List<int> retval = new List<int>();
            using (SqlDataReader dbreader = dbcommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (dbreader.Read())
                {
                    retval.Add(dbreader.GetInt32(0));
                }
            }

            return retval;
        }

        #endregion
    }
}
