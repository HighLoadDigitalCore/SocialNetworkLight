using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SexiLove.Extensions;
using SexiLove.Models;

namespace SexiLove.Controllers
{
    public class EventsController : BaseController
    {
        //
        // GET: /Events/

        [Authorize]
        public ActionResult ChangeGo(int ID)
        {
            var ev =
                DB.EventPeoples.FirstOrDefault(
                    x => (x.FromUserID == CurrentUser.ID || x.ToUserID == CurrentUser.ID) && x.EventID == ID);

            if (ev != null)
            {
                ev.Type = 1;
                if (ev.Status == 1)
                {
                    var related =
                   DB.EventPeoples.Where(
                       x =>
                           x.FromUserID == CurrentUser.ID && x.ToUserID != CurrentUser.ID && x.Type == 1 &&
                           x.EventID == ID);
                    if (related.Any())
                    {
                        DB.EventPeoples.DeleteAllOnSubmit(related);
                    }
                    DB.EventPeoples.DeleteOnSubmit(ev);
                }
                else
                {
                    ev.Status = 1;
                }

            }
            else
            {
                ev = new EventPeople()
                {
                    Status = 1,
                    Type = 1,
                    FromUserID = CurrentUser.ID,
                    SendDate = DateTime.Now,
                    EventID = ID
                };

                DB.EventPeoples.InsertOnSubmit(ev);
            }
            DB.SubmitChanges();
            return PartialView(DB.Events.First(x => x.ID == ID));
        }


        [Authorize]
        public ActionResult Invite(int ID, int UserID, int Type)
        {
            var exist =
                DB.EventPeoples.FirstOrDefault(
                    x =>
                        ((x.FromUserID == CurrentUser.ID && x.ToUserID == UserID) ||
                         (x.ToUserID == CurrentUser.ID && x.FromUserID == UserID)) && x.Type == Type && x.EventID == ID);
            if(exist != null)
                return new ContentResult();


            exist = new EventPeople()
            {
                FromUserID = CurrentUser.ID,
                ToUserID = UserID,
                Type = Type,
                Status = 0,
                SendDate = DateTime.Now,
                EventID = ID
            };

            DB.EventPeoples.InsertOnSubmit(exist);
            DB.SubmitChanges();
            return new ContentResult();
        }

        [Authorize]
        public ActionResult ChangeGoDetails(int ID, int Type)
        {

            var ev =
                DB.EventPeoples.FirstOrDefault(
                    x => (x.FromUserID == CurrentUser.ID || x.ToUserID == CurrentUser.ID) && x.EventID == ID);




            if (ev != null)
            {
                ev.Type = Type;
                if (ev.Status == 1)
                {

                    var related =
                        DB.EventPeoples.Where(
                            x =>
                                x.FromUserID == CurrentUser.ID && x.ToUserID != CurrentUser.ID && x.Type == 1 &&
                                x.EventID == ID);
                    if (related.Any())
                    {
                        DB.EventPeoples.DeleteAllOnSubmit(related);
                    }
                    DB.EventPeoples.DeleteOnSubmit(ev);

                }
                else
                {
                    ev.Status = 1;
                }

            }
            else
            {
                ev = new EventPeople()
                {
                    Status = 1,
                    Type = Type,
                    FromUserID = CurrentUser.ID,
                    SendDate = DateTime.Now,
                    EventID = ID
                };

                DB.EventPeoples.InsertOnSubmit(ev);
            }
            DB.SubmitChanges();
            return PartialView(DB.Events.First(x => x.ID == ID));
        }
        public ActionResult Index(string cat)
        {
            return View();
        }
        public ActionResult View(string cat)
        {
            return View();
        }

        [HttpGet]
        public ActionResult EventsSearch(string cat)
        {
            var dating = new EventsSearch(CurrentUser);
            dating.CatUrl = cat;
            if (Session["EventsSearch"] is EventsSearch)
            {
                dating = (EventsSearch)Session["EventsSearch"];
                Session["EventsSearch"] = null;
            }

            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult EventsSearch(EventsSearch dating, string cat)
        {
            dating.CatUrl = cat;
            dating.Search(DB);
            return PartialView(dating);
        }

        [HttpGet]
        public ActionResult EventsSearchDetail(int ID)
        {
            ViewBag.Event = DB.Events.FirstOrDefault(x => x.ID == ID);
            var dating = new EventsSearch(CurrentUser);
            if (Session["EventsSearch"] is EventsSearch)
            {
                dating = (EventsSearch)Session["EventsSearch"];
                Session["EventsSearch"] = null;
            }

            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult EventsSearchDetail(EventsSearch dating)
        {
            dating.LoadCats();
            Session["EventsSearch"] = dating;
            ViewBag.Redirect = Url.Action("Index");
            return PartialView(dating);
        }

        public ActionResult DeleteInvite(int id)
        {
            var invite = DB.EventPeoples.FirstOrDefault(x => x.ID == id);
            if (invite != null && invite.FromUserID == CurrentUser.ID)
            {
                invite =
                    DB.EventPeoples.FirstOrDefault(
                        x => x.FromUserID == CurrentUser.ID && x.ToUserID != null && x.EventID == invite.EventID);
                if (invite != null)
                {
                    DB.EventPeoples.DeleteOnSubmit(invite);
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("MyIndex");
        }


        [Authorize]
        public ActionResult MyIndex()
        {
            return View();
        }
        [Authorize]
        public ActionResult My(string Search, int Order = 0)
        {
            var friendship = new MyEvents(DB, Search, Order, CurrentUser);
            return PartialView(friendship);
        }

        public ActionResult Status(int id, int status)
        {
            var ep = DB.EventPeoples.FirstOrDefault(x => x.ID == id);
            if (ep != null)
            {
                ep.Status = status;
                ep.Type = 1;
                DB.SubmitChanges();
            }
            return RedirectToAction("MyIndex", "Events");
        }

        public ActionResult Remove(int id)
        {
            var ep = DB.EventPeoples.FirstOrDefault(x => x.ID == id);
            if (ep != null)
            {
                if (ep.Type == 0)
                {
                    DB.EventPeoples.DeleteOnSubmit(ep);
                    DB.SubmitChanges();
                }
                else
                {
                    DB.EventPeoples.DeleteOnSubmit(ep);
                    if (
                        DB.EventPeoples.Any(
                            x =>
                                x.FromUserID == CurrentUser.ID && x.ToUserID == null && x.EventID == ep.EventID &&
                                x.Type == 1))
                    {
                        var another =
                            DB.EventPeoples.Where(
                                x => x.FromUserID == ep.FromUserID && x.EventID == ep.EventID && x.Type == 1);
                        if (another.Any())
                        {

                            DB.EventPeoples.DeleteAllOnSubmit(another);

                        }
                    }
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("MyIndex");
        }
    }
}
