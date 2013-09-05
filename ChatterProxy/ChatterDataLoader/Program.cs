using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;

namespace ChatterDataLoader
{
    class Program
    {
        string _url;
        string _username;
        string _password;
        string _token;

        Program()
        {
            _url = ConfigurationSettings.AppSettings["SalesForceUrl"];
            _username = ConfigurationSettings.AppSettings["SalesForceUserName"];
            _password = ConfigurationSettings.AppSettings["SalesForcePassword"];
            _token = ConfigurationSettings.AppSettings["SalesForceToken"];
        }

        static void Main(string[] args)
        {
            try
            {
                Program program = new Program();
                program.Run();
            }
            catch(Exception ex) {
                Console.Out.WriteLine(ex);
            }
        }

        void Run()
        {
            ChatterService.IChatterSoapService service = new ChatterService.ChatterSoapService(_url);
            service.AllowUntrustedConnection();
            service.Login(_username, _password, _token);

            ProfilesDataContext dc = new ProfilesDataContext();

            List<user> rs =(from p in dc.GetTable<user>() where (p.InternalUserName != null && p.PersonID != null) select p).ToList<user>();

            StreamWriter errors = new StreamWriter("errors.csv", false);
            errors.WriteLine("Name, Person Id, Employee Id, Error");

            StreamWriter processed = new StreamWriter("processed.csv", false);
            processed.WriteLine("Name, Person Id, Employee Id");

            int count = 0;
            try
            {
                foreach (user u in rs)
                {
                    try
                    {
                        service.CreateResearchProfile(u.InternalUserName);
                        processed.WriteLine(u.FirstName + " " + u.LastName + "," + u.PersonID + "," + u.InternalUserName);
                    }
                    catch (Exception ex)
                    {
                        errors.WriteLine(u.FirstName + " " + u.LastName + "," + u.PersonID + "," + u.InternalUserName + ",\"" + ex.Message + "\"");
                    }
                    count++;
                    if (count % 100 == 0)
                    {
                        Console.Out.WriteLine("Processed " + count + " Research Profiles");
                    }
                }
            }
            finally
            {
                processed.Close();
                errors.Close();
                Console.Out.WriteLine("Processed " + count + " Research Profiles");
            }
        }
    }
}
