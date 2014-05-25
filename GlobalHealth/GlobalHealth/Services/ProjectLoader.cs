using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using log4net;
using Newtonsoft.Json;
using UCSF.GlobalHealth.Domain;

namespace UCSF.GlobalHealth.Services
{
	class ProjectLoader
	{
		private static string SQL_SELECT_APP_ID = "select appId from [ORNG.].[Apps] where name = @name";
		private static string SQL_ALL_DELETE_PROJECTS = "delete from [ORNG.].[AppData] where appId = @appId";
		private static string SQL_ALL_DELETE_APP_REGISTRY = "delete from [ORNG.].AppRegistry where appId = @appId";
		private static string SQL_SELECT_NODE_ID = "select nodeid from [UCSF.].vwPerson where InternalUsername = @employeeId";
		private static string SQL_INSERT_APP_DATA = "insert [ORNG.].[AppData] (NodeID, AppID, keyName, value, createdDT, updatedDT) values(@nodeId, @appId, @key, @val, GetDate(), GetDate())";
		private static string SQL_INSERT_APP_REGISTRY = "insert [ORNG.].AppRegistry (NodeID, AppID, createdDT) values(@nodeId, @appId, GetDate())";

		private ILog Log { get; set; }

		public String Url { get; set; }
		public String ApplicationName { get; set; }

		private SqlCommand GetNodeIdCmd { get; set; }
		private SqlCommand InsertDataCmd { get; set; }
		private SqlCommand InsertAppRegistryCmd { get; set; }

		public ProjectLoader(string url, string applicationName)
		{
			Log = LogManager.GetLogger("UCSF.GlobalHealth.ProjectLoader");
			this.Url = url;
			this.ApplicationName = applicationName;
		}

		public IList<Project> Load() {
			HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
				{
					if (response.StatusCode != HttpStatusCode.OK) 
					{
						throw new Exception(String.Format("Server error (HTTP {0}: {1}).", response.StatusCode, response.StatusDescription));
					}

					String json;
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					{
						json = sr.ReadToEnd();
					}

					JavaScriptSerializer serializer = new JavaScriptSerializer();
					serializer.RegisterConverters(new JavaScriptConverter[] { new ProjectConverter() });
					Project[] projects = serializer.Deserialize<Project[]>(json);

					return projects.ToList();
				}
			}
		}

		public void Save(IList<Project> projects) {
			string connStr = ConfigurationManager.ConnectionStrings["UCSF.Data.Properties.Settings.UCSFConnectionString"].ConnectionString;

			IDictionary<String, IList<Project>> employeeProjects = PrepareProjects(projects);

			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				PrepareStetements(conn);

				int applicationId = GetApplicationId(conn, ApplicationName);
				DeleteProjects(conn, applicationId);
				DeleteAppRegistry(conn, applicationId);
				foreach (string employeeId in employeeProjects.Keys)
				{
					Save(conn, applicationId, employeeId, employeeProjects[employeeId]);
				}

			}
		}

		protected IDictionary<String, IList<Project>> PrepareProjects(IList<Project> projects) {
			IDictionary<String, IList<Project>> employeeProjects = new Dictionary<String, IList<Project>>();
			foreach(Project project in projects) {
				IList<Project> list;
				if (String.IsNullOrEmpty(project.EmployeeId))
				{
					Log.ErrorFormat("EmployeeId is empty, project id={0}", project.Id);
					continue;
				}
				if(!employeeProjects.TryGetValue(project.EmployeeId, out list)) {
					list = new List<Project>();
					employeeProjects.Add(project.EmployeeId, list);
				}
				list.Add(project);
			}
			return employeeProjects;
		}

		protected int GetApplicationId(SqlConnection conn, string appName) {
			SqlCommand dbcommand = new SqlCommand(SQL_SELECT_APP_ID, conn);
			dbcommand.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = appName;

			dbcommand.Prepare();
			var id = dbcommand.ExecuteScalar();
			if (id == null) {
				throw new Exception("Application was not found, application name=" + appName);
			}
			Log.InfoFormat("Using application id={0}, name={1}", id, appName);
			return (int)id;
		}

		protected void DeleteProjects(SqlConnection conn, int applicationId)
		{
			SqlCommand dbcommand = new SqlCommand(SQL_ALL_DELETE_PROJECTS, conn);

			dbcommand.Parameters.Add("@appId", SqlDbType.Int, 0).Value = applicationId;

			dbcommand.Prepare();
			var cnt = dbcommand.ExecuteNonQuery();
			Log.InfoFormat("Deleted {0} records", cnt);
		}

		protected void DeleteAppRegistry(SqlConnection conn, int applicationId)
		{
			SqlCommand dbcommand = new SqlCommand(SQL_ALL_DELETE_APP_REGISTRY, conn);

			dbcommand.Parameters.Add("@appId", SqlDbType.Int, 0).Value = applicationId;

			dbcommand.Prepare();
			var cnt = dbcommand.ExecuteNonQuery();
			Log.InfoFormat("Deleted aapp registry records, count={0}", cnt);
		} 

		protected void Save(SqlConnection conn, int applicationId, string employeeId, IList<Project> projects) {
            long? nodeId = GetNodeId(employeeId);
            if (!nodeId.HasValue)
            {
				Log.WarnFormat("Person was not found employeeId={0}, projects={1}", employeeId, projects.Count);
				return; 
			}
            Log.InfoFormat("Saving projects for userId={0}, employeeId={1}, projects={2}", nodeId, employeeId, projects.Count);

			InsertDataCmd.Parameters.Clear();

			InsertDataCmd.Parameters.Add("@appId", SqlDbType.Int, 0).Value = applicationId;
            InsertDataCmd.Parameters.Add("@nodeId", SqlDbType.BigInt, 0).Value = nodeId;
			InsertDataCmd.Parameters.Add("@key", SqlDbType.VarChar, 100).Value = "gh_n";
			InsertDataCmd.Parameters.Add("@val", SqlDbType.VarChar, 100).Value = projects.Count;

			var cnt = InsertDataCmd.ExecuteNonQuery();

			int i = 0;
			projects = projects.OrderBy(o => o.EndDate).ToList();
			foreach(Project project in projects) {
				InsertDataCmd.Parameters.Clear();

				InsertDataCmd.Parameters.Add("@appId", SqlDbType.Int, 0).Value = applicationId;
                InsertDataCmd.Parameters.Add("@nodeId", SqlDbType.BigInt, 0).Value = nodeId;
				InsertDataCmd.Parameters.Add("@key", SqlDbType.VarChar, 100).Value = "gh_" + i++;
				InsertDataCmd.Parameters.Add("@val", SqlDbType.VarChar, 4000).Value = GetJson(project);

				var cnt2 = InsertDataCmd.ExecuteNonQuery();
			}

			AddAppRegistry(conn, applicationId, nodeId.Value);
		}


		private void AddAppRegistry(SqlConnection conn, int applicationId, long nodeId)
		{
			InsertAppRegistryCmd.Parameters.Clear();

			InsertAppRegistryCmd.Parameters.Add("@appId", SqlDbType.Int, 0).Value = applicationId;
			InsertAppRegistryCmd.Parameters.Add("@nodeId", SqlDbType.BigInt, 0).Value = nodeId;

			InsertAppRegistryCmd.ExecuteNonQuery();
			Log.InfoFormat("Added app registry appId={0}, nodeId={1}", applicationId, nodeId);
		}

		private string GetJson(Project project) {
			string json = JsonConvert.SerializeObject(project);
			return json;
		}

		protected long? GetNodeId(String employeeId) {
			GetNodeIdCmd.Parameters.Clear();
			GetNodeIdCmd.Parameters.Add("@employeeId", SqlDbType.VarChar, 100).Value = employeeId;

			var userId = GetNodeIdCmd.ExecuteScalar();
			if (userId == null) {
				return null;
			}
						
			return (long)userId;
		}

		protected void PrepareStetements(SqlConnection conn)
		{
			GetNodeIdCmd = new SqlCommand(SQL_SELECT_NODE_ID, conn);
			GetNodeIdCmd.Prepare();

			InsertDataCmd = new SqlCommand(SQL_INSERT_APP_DATA, conn);
			InsertDataCmd.Prepare();

			InsertAppRegistryCmd = new SqlCommand(SQL_INSERT_APP_REGISTRY, conn);
			InsertAppRegistryCmd.Prepare();
		}

	}

}
