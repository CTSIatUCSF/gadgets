using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommandLine;
using UCSF.Business.DataImporter;
using UCSF.Business.Web;
using log4net;

namespace UCSF.GrantLoader
{
    class Program
    {
        internal static ILog log;
        public static ILog Log
        {
            get { return log; }
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
            string fileName = null;
            if (!options.CheckForUpdates && !options.Validate && !options.CheckForUpdatesNoBulk)
            {
                if (options.FileName == null || options.FileName.Count == 0)
                {
                    Options.ShowUsage();
                    return;
                }

                fileName = options.FileName[0];
                if (!File.Exists(fileName))
                {
                    Options.ShowUsage();
                    log.InfoFormat("File {0} does not exists.", fileName);
                    Environment.ExitCode = 1;
                    return;
                }
            }

            Stopwatch sw = new Stopwatch();

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
                log.InfoFormat("{0} Records imported. {1} Errors", bi.TotalProcessed, bi.ErrorsCount);
            }
            else if (options.CheckForUpdates)
            {
                WebDownloader d = new WebDownloader();
                d.CheckForUpdates(options.OrgName, true);
            }
            else if (options.CheckForUpdatesNoBulk)
            {
                WebDownloader d = new WebDownloader();
                d.CheckForUpdates(options.OrgName, false);
            }
            else if (!options.Validate)
            {
                GrantImporter gi = new GrantImporter();
                gi.ImportData(fileName, options.OrgName, null);
                log.InfoFormat("{0} Records imported. {1} Errors", gi.TotalRecords, gi.ErrorsCount);
            }
            if (options.Validate)
            {
                GrantOnlineValidator validator = new GrantOnlineValidator();
                validator.ValidateGrants();
                log.InfoFormat("Total {0} Record(s) validated. {1} invalid grant(s)", validator.TotalProcessed, validator.InvalidGrants);
                if(validator.ErrorsCount > 0)
                {
                    log.InfoFormat("Total {0} Errors(s)", validator.ErrorsCount);
                }
            }

            sw.Stop();
            log.InfoFormat("Total time: {0:0.#} seconds.", sw.Elapsed.TotalSeconds);
            log.Info("Done.");
            Environment.ExitCode = 0;
        }
    }
}