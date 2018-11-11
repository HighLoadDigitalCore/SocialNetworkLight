using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{
    public partial class FAQ
    {
        public bool IsPost { get; set; }
        public string Message { get; set; }
    }
    public partial class BlockedUser
    {
        public bool IsPost { get; set; }

        public string StartIPStr
        {
            get { return StartIP.ToIPString(); }
            set { StartIP = value.ToIPInt(); }
        }
        public string EndIPStr
        {
            get { return EndIP.ToIPString(); }
            set { EndIP = value.ToIPInt(); }
        }
    }

    public partial class Town
    {
        public bool IsPost { get; set; }
    }


    public partial class TextPage
    {
        public bool IsPost { get; set; }
        public static string GetPageURL(string url)
        {
            var db = new xDBDataContext();
            var page = db.TextPages.FirstOrDefault(x => x.URL == url);
            if (page == null)
            {
                page = new TextPage() {URL = url, Text = "", Name = ""};
                db.TextPages.InsertOnSubmit(page);
                db.SubmitChanges();
            }
            return "/Home/Text?page=" + page.URL;
        }
    }
}