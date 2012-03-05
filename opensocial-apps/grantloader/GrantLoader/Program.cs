using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommandLine;
using UCSF.Business.DataImporter;
using log4net;

namespace UCSF.GrantLoader
{
    class Program
    {
        private static ILog log;

        internal sealed class Options
        {
            [Option("n", "name", Required = false, HelpText = "Organization name to filter data")]
            public string OrgName = String.Empty;

            [Option("b", "bcp", Required = false, HelpText = "Use bcp for csv data import")]
            public bool UseBCP = false;

            [Option("l", "bulk", Required = false, HelpText = "Use bulk insert for xml data import")]
            public bool UseBULK = false;

            [ValueList(typeof(List<string>), MaximumElements = 1)]
            public IList<string> FileName;

            [HelpOption(HelpText = "Dispaly this help screen.")]
            internal static void ShowUsage()
            {
                log.Info("USAGE: UCSF.GrantLoader.exe <file_name> [-b] [-l] [-n Name]");
                log.Info("[-n Name] optional Filter data by ORG_NAME attribute value. This param is ignored when doing bulk insert.");
                log.Info("[-b] Use BCP tool for csv data import.");
                log.Info("[-l] Use bulk insert for xml data import.");
            }
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log = LogManager.GetLogger("UCSF.GrantLoader");

            try
            {
                Options options = new Options();
                ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
                if (!parser.ParseArguments(args, options))
                {
                    Options.ShowUsage();
                    Environment.Exit(1);
                }

                ExecTasks(options);
#if DEBUG
                Console.ReadKey();
#endif
            }
            catch(Exception ex)
            {
                log.Info("Unhandled exception.");
                log.Debug("Unhandled exception", ex);
            }
        }

        private static void ExecTasks(Options options)
        {
            if(options.FileName== null || options.FileName.Count ==0)
            {
                Options.ShowUsage();
                return;
            }

            string fileName = options.FileName[0];
            if (!File.Exists(fileName))
            {
                Options.ShowUsage();
                log.InfoFormat("File {0} does not exists.", fileName);
                Environment.ExitCode = 1;
                return;
            }

            Stopwatch sw = new Stopwatch();

            log.InfoFormat("Import started for file {0}.", fileName);
            sw.Start();

            if(options.UseBCP)
            {
                BCPImporter bi = new BCPImporter();
                bi.Import(fileName);
            }
            else if(options.UseBULK)
            {
                BulkImporter bi = new BulkImporter();
                bi.ImportData(fileName, options.OrgName, null);
                log.InfoFormat("{0} Records imported. {1} Errors", bi.TotalRecords, bi.ErrorsCount);
            }
            else
            {
                GrantImporter gi = new GrantImporter();
                gi.ImportData(fileName, options.OrgName, null);
                log.InfoFormat("{0} Records imported. {1} Errors", gi.TotalRecords, gi.ErrorsCount);
            }
            sw.Stop();
            log.InfoFormat("Total time: {0:0.#} seconds.", sw.Elapsed.TotalSeconds);
            log.Info("Done.");
            Environment.ExitCode = 0;
        }
    }
}