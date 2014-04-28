using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UCSF.GlobalHealth.Domain
{
	[DataContract]
	class Project
	{
		[DataMember(Name = "Id")]
		public virtual String Id { get; set; }

		[DataMember(Name = "Title")]
		public virtual String Title { get; set; }

		[DataMember(Name = "Locations")]
		public virtual String[] Locations { get; set; }

		[DataMember(Name = "EmployeeId")]
		public virtual String EmployeeId { get; set; }

		[DataMember(Name = "Departments")]
		public virtual String[] Department { get; set; }

		[DataMember(Name = "StartDate")]
		public virtual DateTime StartDate { get; set; }

		[DataMember(Name = "EndDate")]
		public virtual DateTime EndDate { get; set; }

		[DataMember(Name = "InvestigatorId")]
		public virtual String InvestigatorId { get; set; }
	}
}
