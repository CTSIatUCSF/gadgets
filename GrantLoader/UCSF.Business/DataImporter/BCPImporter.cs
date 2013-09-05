using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UCSF.Data;
using log4net;

namespace UCSF.Business.DataImporter
{
    public class BCPImporter
    {
        private ILog log;

        public BCPImporter()
        {
            log = LogManager.GetLogger(GetType());
        }

        public void Import(string file)
         {
             log.InfoFormat("Import started for file {0}.", file);
             if(ConfigurationManager.ConnectionStrings["UCSF.Data.Properties.Settings.UCSFConnectionString"].
                     ConnectionString != null)
             {
                 string connectionString =
                     ConfigurationManager.ConnectionStrings["UCSF.Data.Properties.Settings.UCSFConnectionString"].ConnectionString;

                 string serverName = GetConnectionSetting("Data Source", connectionString);
                 string database = GetConnectionSetting("Initial Catalog", connectionString);

                 //bcp UCSF.dbo.GrantData in RePORTER_PRJ_C_FY2012_033.csv -f rawdata.xml -E -T -S .\sqlexpress
                 string output = RunBCP(String.Format("{0}.[UCSF].agGrantData in {1} -f rawdata.xml -m 1000 -T -S {2}", database, file, serverName));
                 log.Info(output);
             }

         }

        private string GetConnectionSetting(string name, string connectionString)
        {
            string[] settings = connectionString.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            return (from it in settings where it.StartsWith(name) select it.Replace(name+"=","").Trim()).FirstOrDefault();
        }

        public static string RunBCP(string args)
        {
            string returnvalue = string.Empty;

            ProcessStartInfo info = new ProcessStartInfo("bcp.exe");
            info.UseShellExecute = false;
            info.Arguments = args;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;

            using (Process process = Process.Start(info))
            {
                StreamReader sr = process.StandardOutput;
                returnvalue = sr.ReadToEnd();
            }

            return returnvalue;
        }
    }
}