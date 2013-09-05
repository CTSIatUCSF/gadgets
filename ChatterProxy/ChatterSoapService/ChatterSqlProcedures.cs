using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace ChatterService
{
    public class ChatterSqlProcedures
    {

        public ChatterSqlProcedures()
        {
        }


        [SqlProcedure]
        public static void CreateActivity(SqlString url, SqlString username, SqlString password, SqlString token, SqlDateTime createdDT, SqlBoolean externalMessage, SqlString employeeId, SqlString actUrl, SqlString actTitle, SqlString actBody)
        {
            IChatterSoapService service = new ChatterSoapService(url.Value);
            service.AllowUntrustedConnection();
            service.Login(username.Value, password.Value, token.Value);

            service.CreateProfileActivity(employeeId.IsNull ? null : employeeId.Value, actUrl.IsNull ? null : actUrl.Value, actTitle.IsNull ? null : actTitle.Value, actBody.IsNull ? null : actBody.Value, createdDT.Value);
            if (externalMessage.IsTrue)
            {
                service.CreateExternalMessage(actUrl.IsNull ? null : actUrl.Value, actTitle.IsNull ? null : actTitle.Value, actBody.IsNull ? null : actBody.Value, employeeId.IsNull ? null : employeeId.Value);
            }
        }


    }
}
