using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using SexiLove.Extensions;
using SexiLove.Models;
using WebMatrix.WebData;

namespace SexiLove.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Text(string page)
        {
            var p = DB.TextPages.First(x => x.URL.ToLower() == page.ToLower());
            if (p == null)
                return RedirectToAction("NotFound");
            return View(p);
        }
        public ActionResult SaveTown(int townID)
        {
            if (CurrentUser == null)
            {
                return new ContentResult(){Content = "0"};
            }
            var user = DB.Users.First(x => x.ID == CurrentUser.ID);
            user.TownID = townID;
            DB.SubmitChanges();
            return new ContentResult(){Content = "1"};
        }

        public ActionResult Visit()
        {
            if (CurrentUser != null)
            {
                var user = DB.Users.FirstOrDefault(x => x.ID == CurrentUser.ID);
                if (user != null)
                {
                    user.LastVisitDate = DateTime.Now;
                    DB.SubmitChanges();
                }
            }
            return new ContentResult();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult Index()
        {

            DB.ExecuteCommand("update Users set IsVip = 0 where VipEnd is null or Convert(date, VipEnd)  < Convert(date,getdate())");
            DB.ExecuteCommand("update Users set IsUp = 0 where UpEnd is null or Convert(date, UpEnd) < Convert(date,getdate())");
            DB.ExecuteCommand("update Users set IsHidden = 0 where HiddenEnd is null or Convert(date,HiddenEnd) < Convert(date,getdate())");
            DB.ExecuteCommand("update Places set IsVip = 0 where VipEnd is null or Convert(date,VipEnd) < Convert(date,getdate())");
            if (WebSecurity.IsAuthenticated)
            {
                //return RedirectToAction("Index", "Cabinet");
            }
            return View();
        }

        public ActionResult TownList(int? townID)
        {
            ViewBag.SelectedTown = townID;
            return PartialView(DB.Towns.ToList());
        }


        [HttpPost]
        public ActionResult People(int city, int sex, string years)
        {
            var ages = years.Split<int>("-").ToList();
            var ds = new DatingSearch()
            {
                TownID = city,
                Sex = sex == 1,
                MinAge = ages[0],
                MaxAge = ages[1]
            };
            Session["DatingSearch"] = ds;
            return Redirect(Url.Action("Index", "Dating"));
        }    
        [HttpPost]
        public ActionResult Meeting(int city, int sex, string years)
        {
            var ages = years.Split<int>("-").ToList();
            var ds = new MeetingShortSearch()
            {
                TownID = city,
                Sex = sex == 1,
                MinAge = ages[0],
                MaxAge = ages[1]
            };
            Session["MeetingShortSearch"] = ds;
            return Redirect(Url.Action("Index", "Meeting"));
        }


        public ActionResult FaqList()
        {
            var faq =
                DB.FAQs.Where(x => x.Visible && x.Qst.Trim() != "" && x.Ans.Trim() != "")
                    .OrderByDescending(x => x.CreateDate).ToList();
            return PartialView(faq);
        }

        public ActionResult PageBg()
        {
            return PartialView(CurrentUser);
        }

        [HttpGet]
        public ActionResult QstForm()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult QstForm(string Message)
        {
            if (Message.IsNullOrEmpty())
            {
                ViewBag.Error = "Необходимо заполнить сообщение";
                return PartialView();
            }

            var qst = new FAQ() {Ans = "", CreateDate = DateTime.Now, Qst = Message, Visible = false};
            if (CurrentUser != null)
            {
                qst.UserID = CurrentUser.ID;
            }
            DB.FAQs.InsertOnSubmit(qst);
            DB.SubmitChanges();
            ViewBag.Success = true;
            return PartialView();
        }
    }
}
