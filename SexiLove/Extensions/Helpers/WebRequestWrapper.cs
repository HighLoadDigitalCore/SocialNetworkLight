using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SexiLove.Extensions.Helpers
{
    public class WebRequestWrapper
    {
        private string url;
        public WebRequestWrapper(string url)
        {
            this.url = url;
        }

        public string Post(string postString, bool isCompressed = false)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml";
            webRequest.ContentLength = postString.Length;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            if (isCompressed)
            {
                webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            }

            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
            string responseData = responseReader.ReadToEnd();

            responseReader.Close();
            webRequest.GetResponse().Close();
            return responseData;
        }

        public void PostAndSaveResponse(string postString, string targetFileRelative, bool isCompressed = false)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml";
            webRequest.ContentLength = postString.Length;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            if (isCompressed)
            {
                webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            }


            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            var stream = webRequest.GetResponse().GetResponseStream();

            FileStream fs = new FileStream(HttpContext.Current.Server.MapPath(targetFileRelative), FileMode.Create);

            byte[] buffer = new byte[4096];
            int read = 0;
            do
            {
                read = stream.Read(buffer, 0, 4096);
                if (read > 0)
                    fs.Write(buffer, 0, read);

            } while (read > 0);

            fs.Close();
            webRequest.GetResponse().Close();
        }

    }
}