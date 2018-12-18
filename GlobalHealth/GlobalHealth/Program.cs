using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using UCSF.GlobalHealth.Domain;
using UCSF.GlobalHealth.Services;

namespace UCSF.GlobalHealth
{
	class Program
	{

		static int Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();
			ILog log = LogManager.GetLogger("UCSF.GlobalHealth");
            int returned = 0;
			try
			{
				ProjectLoader loader = new ProjectLoader(
					ConfigurationManager.AppSettings["GlobalHealth.Projects.Url"],
					ConfigurationManager.AppSettings["GlobalHealth.ApplicationName"],
                    ConfigurationManager.AppSettings["GlobalHealth.Url.Timeout"],
                    ConfigurationManager.AppSettings["GlobalHealth.ExternalID"]);

				IList<Project> projects = loader.Load();
				log.InfoFormat("Recieved {0} projects", projects.Count);

				loader.Save(projects);

				log.Info("All projects saved successfully");
			}
        
catch (Exception ex) {
				log.Error("Loading global health projects failed.", ex);
                returned = 7;
			}
            return returned;
		}
	}
}
