using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using UCSF.Business.DataImporter;
using UCSF.Data;
using UCSF.Framework.Utils;
using log4net;
using log4net.Core;

namespace UCSF.Business
{
    public class DataManager
    {
        private readonly UCSDDataContext context;

        private static ILog log;

        public DataManager()
        {
            log = LogManager.GetLogger(GetType());
            context = new UCSDDataContext();
        }

        public UCSDDataContext Context
        {
            get { return context; }
        }
    }
}