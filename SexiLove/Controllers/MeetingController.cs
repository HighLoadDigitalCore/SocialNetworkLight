using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SexiLove.Extensions;
using SexiLove.Models;

namespace SexiLove.Controllers
{
    public class MeetingController : BaseController
    {


        [Authorize]
        public ActionResult WishToMeet(int ID)
        {
            var meet = DB.Meetings.FirstOrDefault(x => x.ID == ID);
            if (meet != null)
            {
                var me = DB.MeetingPeoples.FirstOrDefault(x => x.MeetingID == meet.ID && x.UserID == CurrentUser.ID);
                if (me == null)
                {
                    var p = new MeetingPeople()
                    {
                        Status = 0,
                        UserID = CurrentUser.ID,
                        MeetingID = meet.ID,
                        SendDate = DateTime.Now
                    };
                    DB.MeetingPeoples.InsertOnSubmit(p);
                    DB.SubmitChanges();
                }
                else
                {
                    DB.MeetingPeoples.DeleteOnSubmit(me);
                    DB.SubmitChanges();
                }
            }
            return new ContentResult();
        }

        public ActionResult UserMeetings(int UserID)
        {
            var list =
                DB.Meetings.Where(x => x.Author == UserID && x.Date.HasValue && x.Date.Value.Date >= DateTime.Now.Date).OrderBy(x => x.Date.Value.Date);
            return PartialView(list.ToList());
        }

        //
        // GET: /Meeting/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShortSearch()
        {
            var dating = new MeetingShortSearch(CurrentUser);
            if (Session["MeetingShortSearch"] is MeetingShortSearch)
            {
                dating = (MeetingShortSearch)Session["MeetingShortSearch"];
                Session["MeetingShortSearch"] = null;
            }
            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult ShortSearch(MeetingShortSearch dating)
        {
            dating.Search(DB);
            return PartialView(dating);
        }

        [HttpGet]
        public ActionResult FullSearch()
        {
            var dating = new MeetingFullSearch(CurrentUser);
            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult FullSearch(MeetingFullSearch dating)
        {
            dating.Search(DB);
            return PartialView(dating);
        }


        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Places = DB.Places.Where(x=> x.Approved).ToList();
            return View(new Meeting() { TargetSex = !(CurrentUser.Sex ?? true) });
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Meeting meeting)
        {
            ViewBag.Places = DB.Places.Where(x=> x.Approved).ToList();
            meeting.IsPost = true;

            if (meeting.TimeStr.IsFilled())
            {
                try
                {
                    TimeSpan span = TimeSpan.ParseExact(meeting.TimeStr, "g", CultureInfo.CurrentCulture);
                    meeting.Time = span;
                }
                catch
                {
                    meeting.TimeStr = "";
                    meeting.Time = null;
                }

            }

            if (!meeting.Date.HasValue /*|| !meeting.Time.HasValue*/ || meeting.PlaceID == 0 || meeting.Type.IsNullOrEmpty() /*|| meeting.TimeStr.IsNullOrEmpty()*/)
            {
                return View(meeting);
            }

            meeting.Author = CurrentUser.ID;
            var nm = new MeetingPeople()
            {
                Meeting = meeting,
                UserID = CurrentUser.ID,
                Status = 1,
                SendDate = DateTime.Now
            };
            DB.MeetingPeoples.InsertOnSubmit(nm);

/*
            if (meeting.PlaceID == 0)
            {
                meeting.PlaceID = null;
            }
*/

            DB.Meetings.InsertOnSubmit(meeting);

            DB.SubmitChanges();

            var friends = meeting.FriendList.Split<int>(";");
            foreach (var friend in friends)
            {
                DB.MeetingPeoples.InsertOnSubmit(new MeetingPeople()
                {
                    MeetingID = meeting.ID,
                    SendDate = DateTime.Now,
                    Status = 1,
                    UserID = friend
                });
            }
            DB.SubmitChanges();
            return RedirectToAction("My");
        }

        [HttpGet]
        [Authorize]
        public ActionResult My()
        {
            return View();
        }
        [Authorize]
        public ActionResult MeetAccept(int ID, int Target)
        {
            var p = DB.MeetingPeoples.FirstOrDefault(x => x.ID == ID);
            if (p != null)
            {
                p.Status = Target;
                DB.SubmitChanges();
            }
            return RedirectToAction("My");
        }
        [Authorize]
        public ActionResult DeleteMeet(int ID)
        {
            var people = DB.MeetingPeoples.Where(x => x.MeetingID == ID && x.UserID == CurrentUser.ID);
            if (people.Any())
            {
                DB.MeetingPeoples.DeleteAllOnSubmit(people);
                DB.SubmitChanges();
            }


            var p = DB.Meetings.FirstOrDefault(x => x.ID == ID);
            if (p != null)
            {
                if (p.Author == CurrentUser.ID)
                {
                    var ppl = DB.MeetingPeoples.Where(x => x.MeetingID == ID);
                    if (ppl.Any())
                    {
                        DB.MeetingPeoples.DeleteAllOnSubmit(ppl);
                        DB.SubmitChanges();
                    }

                    DB.Meetings.DeleteOnSubmit(p);
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("My");
        }


        public JsonResult PlacesList(string term)
        {
            var places = DB.Places.Where(x => x.Approved && x.Name.ToLower().StartsWith(term)).OrderByDescending(x => x.PlaceRatings.Where(z=> z.Name == "").Sum(z => z.Rating)).ToList();
            return Json(places.Select(x => new { label = x.Name, value = x.ID.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult FriendsList(string term)
        {
            var friends =
                DB.Friends.Where(
                    x => x.IsRead && (x.FromUserID == CurrentUser.ID || x.ToUserID == CurrentUser.ID) && x.Status == 1)
                    .ToList()
                    .Select(
                        x =>
                            new
                            {
                                Value = x,
                                Name =
                                    (x.ToUserID == CurrentUser.ID ? x.FromUser.NameAndSurname : x.ToUser.NameAndSurname)
                            })
                    .OrderBy(x => x.Name)
                    .Select(x => x.Value)
                    .ToList();

            return
                Json(
                    friends.Select(x => new { label = x.GetFriendName(CurrentUser), value = x.GetUserID(CurrentUser) }).ToArray(),
                    JsonRequestBehavior.AllowGet);
        }
    }
}
