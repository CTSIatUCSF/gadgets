using System;
using System.Collections.Generic;
using CommandLine;
using log4net;

namespace UCSF.GrantLoader
{
    internal sealed class Options
    {
        [Option("n", "name", Required = false, HelpText = "Organization name to filter data")]
        public string OrgName = String.Empty;

        [Option("b", "bcp", Required = false, HelpText = "Use bcp for csv data import")]
        public bool UseBCP = false;

        [Option("l", "bulk", Required = false, HelpText = "Use bulk insert for xml data import")]
        public bool UseBULK = false;

        [Option("u", "updates", Required = false, HelpText = "Check for new projects files in Exporter Catalog.")]
        public bool CheckForUpdates = false;

        [Option("U", "updates", Required = false, HelpText = "Check for new projects files in Exporter Catalog and do NOT use bulk import.")]
        public bool CheckForUpdatesNoBulk = false;

        [Option("v", "validate", Required = false, HelpText = "Validate imported projects in Exporter Catalog.")]
        public bool Validate = false;

        [ValueList(typeof(List<string>), MaximumElements = 1)]

        #pragma warning disable 0649
        //field is used via reflection from command parser
        public IList<string> FileName;
        #pragma warning restore 0649

        [HelpOption(HelpText = "Dispaly this help screen.")]
        internal static void ShowUsage()
        {
            Program.Log.Info("USAGE: UCSF.GrantLoader.exe [-u] [-U] [file_name] [-b] [-l] [-n Name] [-v]");
            Program.Log.Info("[-n Name] optional Filter data by ORG_NAME attribute value. This param is ignored when doing bulk insert.");
            Program.Log.Info("[-b] Use BCP tool for csv data import.");
            Program.Log.Info("[-l] Use bulk insert for xml data import.");
            Program.Log.Info("[-u] Check for new projects files in Exporter Catalog.");
            Program.Log.Info("[-U] Check for new projects files in Exporter Catalog and do NOT use bulk import.");
            Program.Log.Info("[-v] Validate imported projects in Exporter Catalog.");
        }
    }
}