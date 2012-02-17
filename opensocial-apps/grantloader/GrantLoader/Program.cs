using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UCSF.Business;
using UCSF.Business.DataImporter;
using UCSF.Framework.Utils;
using log4net;

namespace UCSF.GrantLoader
{
    class Program
    {
        private static ILog log;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log = LogManager.GetLogger("UCSF.GrantLoader");

            try
            {
                if (args == null || args.Length == 0)
                {
                    ShowUsage();
                    return;
                }
                if (!File.Exists(args[0]))
                {
                    ShowUsage();
                    log.InfoFormat("File {0} does not exists.", args[0]);
                    return;
                }

                Stopwatch sw = new Stopwatch();

                log.Info("Import started.");
                sw.Start();
                GrantImporter gi = new GrantImporter();
                gi.ImportData(args[0]);
                log.InfoFormat("{0} Records imported. {1} Errors", gi.TotalRecords, gi.ErrorsCount);
                sw.Stop();
                log.InfoFormat("Total time: {0:0.#} seconds.", sw.Elapsed.TotalSeconds);
                log.Info("Done.");
                //Console.ReadKey();
            }
            catch(Exception ex)
            {
                log.Info("Unhandled exception.");
                log.Debug("Unhandled exception", ex);
            }
        }

        private static void ShowUsage()
        {
            log.Info("USAGE: UCSF.GrantLoader.exe <file_name>");
        }
    }
}