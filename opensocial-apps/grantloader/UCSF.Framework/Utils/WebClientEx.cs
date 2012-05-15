using System;
using System.Net;

namespace UCSF.Framework.Utils
{
    public class WebClientEx : WebClient
    {
        private int _timeout = 100;
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private HttpWebRequest webRequest;
        public HttpWebRequest WebRequest
        {
            get { return webRequest; }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            webRequest = base.GetWebRequest(address) as HttpWebRequest;

            if (webRequest != null)
            {
                webRequest.Timeout = Timeout * 1000;
            }

            return webRequest;
        }
         
    }
}