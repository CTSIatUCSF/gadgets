using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace UCSF.GlobalHealth.Domain
{
	public class ProjectConverter : JavaScriptConverter
	{

		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			Project project = new Project();
			foreach (string key in dictionary.Keys)
			{
				switch (key)
				{
					case "nid":
						project.Id = (string)dictionary[key];
						break;
					case "node_field_data_field_investigator_nid":
						project.InvestigatorId = (string)dictionary[key];
						break;
					case "node_title":
						project.Title = (string)dictionary[key];
						break;
					case "Start date":
						if (dictionary[key] is string)
						{
							project.StartDate = DateTime.Parse((string)dictionary[key]);
						}
						break;
					case "End date":
						if (dictionary[key] is string)
						{
							project.EndDate = DateTime.Parse((string)dictionary[key]);
						}
						break;
					case "Location":
						if (dictionary[key] is ArrayList)
						{
							project.Locations = (string[])((ArrayList)dictionary[key]).ToArray(typeof(string));
						}
						break;
					case "Department":
						if (dictionary[key] is ArrayList)
						{
							project.Department = (string[])((ArrayList)dictionary[key]).ToArray(typeof(string));
						}
						break;
					case "Employee ID":
						if (dictionary[key] is IDictionary<string, object>)
						{
							IDictionary<string, object> employee = (IDictionary<string, object>)dictionary[key];
							project.EmployeeId = (string)employee["value"];
						}
						break;
				}
			}
			return project;
		}

		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<Type> SupportedTypes
		{
			get { return GetType().Assembly.GetTypes(); }
		}
	}
}
