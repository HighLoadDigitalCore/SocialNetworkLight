using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SexiLove.Models;

namespace SexiLove.Controllers
{
    public class DatingController : BaseController
    {
        //
        // GET: /Dating/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Search()
        {
            var dating = new DatingSearch(CurrentUser);

            if (Session["DatingSearch"] != null && Session["DatingSearch"] is DatingSearch)
            {
                dating = (DatingSearch) Session["DatingSearch"];
                Session["DatingSearch"] = null;
            }

            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult Search(DatingSearch dating)
        {
            dating.Search(DB);
            return PartialView(dating);
        }

    }
}
