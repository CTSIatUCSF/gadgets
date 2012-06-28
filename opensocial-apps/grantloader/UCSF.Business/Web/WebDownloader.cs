using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Ionic.Zip;
using UCSF.Business.DataImporter;
using UCSF.Data;
using log4net;

namespace UCSF.Business.Web
{
    public class WebDownloader
    {
        public const string EXPORTER_CATALOG = "http://exporter.nih.gov/ExPORTER_Catalog.aspx";
        public const string DOWNLOADS = "Downloads";

        private readonly ILog log;

        public UCSDDataContext DataContext { get; private set; } 

        public WebDownloader()
        {
            DataContext = new UCSDDataContext();
            log = LogManager.GetLogger(GetType());
        }
        
        public void CheckForUpdates(string orgName, bool useBulk)
        {
            string downloadsFolder = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), DOWNLOADS);
            if (!Directory.Exists(downloadsFolder))
            {
                Directory.CreateDirectory(downloadsFolder);
            }

            int totalErrors;
            int totalProcessed = totalErrors = 0;

            log.InfoFormat("Check for updates at {0}", EXPORTER_CATALOG);
            WebClient wc = new WebClient();
            using (MemoryStream ms = new MemoryStream(wc.DownloadData(EXPORTER_CATALOG)))
            {
                ms.Seek(0, SeekOrigin.Begin);
                HtmlDocument doc = new HtmlDocument();
                doc.Load(ms);

                List<string> links = (from link in doc.DocumentNode.SelectNodes("//a[@href]")
                                      select link.Attributes["href"]
                                      into href
                                      where !string.IsNullOrWhiteSpace(href.Value) && href.Value.Contains("_PRJ_X_")
                                      orderby href.Value
                                      select "http://exporter.nih.gov/" + href.Value).ToList();

                bool addToProcessed = false;
                foreach (string link in links)
                {
                    Uri uri = new Uri(link);
                    string fileName = Path.GetFileName(uri.LocalPath);
                    if (!FileProcessed(fileName))
                    {
                        log.InfoFormat("Downloading file {0}", fileName);
                        WebClient d = new WebClient();
                        byte[] compessed = d.DownloadData(uri);
//                        byte[] compessed = File.ReadAllBytes(@"F:\Downloads\RePORTER_PRJ_X_FY2012_016.zip");
                        log.InfoFormat("File '{0}' downloaded to {1} folder.", fileName, downloadsFolder);

                        string zipPath = Path.Combine(downloadsFolder, fileName);
                        File.WriteAllBytes(zipPath, compessed);
                        ZipFile file = new ZipFile(zipPath);
                        foreach (ZipEntry zipEntry in file)
                        {
                            string fName = zipEntry.FileName;
                            zipEntry.Extract(downloadsFolder, ExtractExistingFileAction.OverwriteSilently);
                            log.InfoFormat("Unpacked '{0}' to {1}.", fileName, fName);

                            try
                            {
                                if (useBulk)
                                {
                                    BulkImporter importer = new BulkImporter();
                                    importer.ImportData(Path.Combine(downloadsFolder, fName), orgName);
                                    log.InfoFormat("{0} Records imported. {1} Errors", importer.TotalRecords, importer.ErrorsCount);
                                    totalProcessed = totalProcessed + importer.TotalRecords;
                                    totalErrors = totalErrors + importer.ErrorsCount;
                                    addToProcessed = true;
                                }
                                else
                                {
                                    GrantImporter gi = new GrantImporter();
                                    gi.ImportData(Path.Combine(downloadsFolder, fName), orgName, null);
                                    log.InfoFormat("{0} Records imported. {1} Errors", gi.TotalRecords, gi.ErrorsCount);
                                    totalProcessed = totalProcessed + gi.TotalRecords;
                                    totalErrors = totalErrors + gi.ErrorsCount;
                                    addToProcessed = true;
                                }
                            }
                            catch(Exception ex)
                            {
                                log.Debug("Error running bulk insert.", ex);
                            }
                            finally
                            {
                                //add or not to add?
                                if (addToProcessed)
                                {
                                    AddFileToProcessed(fileName);
                                }
                            }
                        }
                    }
                }
                log.InfoFormat("End of import.");
                log.InfoFormat("Total: {0} Records imported with {1} Errors", totalProcessed, totalErrors);
            }
        }

        private void AddFileToProcessed(string fileName)
        {
            //save file name to db
            DataContext.GrantFiles.InsertOnSubmit(new GrantFile() { FilePK = Guid.NewGuid(), FileName = fileName, Processed = true});
            DataContext.SubmitChanges();
        }

        private bool FileProcessed(string fileName)
        {
            return DataContext.GrantFiles.Any(gf => gf.Processed && gf.FileName.ToLower() == fileName.ToLower());
        }
    }
}