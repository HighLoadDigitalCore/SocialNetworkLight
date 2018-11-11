using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SexiLove.Extensions.Helpers
{
    public class Logger
    {
        public static void WriteToLog(string text, string path = "", bool append = true)
        {
            if (path.IsNullOrEmpty())
                path = "/Temp/log.txt";
            StreamWriter sw = new StreamWriter(HttpContext.Current.Server.MapPath(path), append, Encoding.UTF8);
            sw.WriteLine(text);
            sw.Close();
        }
    }
}