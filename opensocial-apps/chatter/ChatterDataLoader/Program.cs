using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

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
            _url = ChatterService.ChatterService.TEST_SERVICE_URL;
            _username = ConfigurationSettings.AppSettings["username"];
            _password = ConfigurationSettings.AppSettings["password"];
            _token = ConfigurationSettings.AppSettings["token"];
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
            ChatterService.IChatterService service = new ChatterService.ChatterService(_url);
            service.AllowUntrustedConnection();
            service.Login(_username, _password, _token);

            ProfilesDataContext dc = new ProfilesDataContext();

            List<user> rs =(from p in dc.GetTable<user>() where (p.InternalUserName != null && p.PersonID != null) select p).ToList<user>();

            int count = 0;
            try
            {
                foreach (user u in rs)
                {
                    service.CreateResearchProfile(u.InternalUserName);
                    count++;
                    if (count % 100 == 0)
                    {
                        Console.Out.WriteLine("Processed " + count + " Research Profiles");
                    }
                }
            }
            finally
            {
                Console.Out.WriteLine("Processed " + count + " Research Profiles");
            }
        }
    }
}
