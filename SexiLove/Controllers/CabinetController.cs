using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using SexiLove.Extensions;
using SexiLove.Models;
using WebMatrix.WebData;

namespace SexiLove.Controllers
{
    public class CabinetController : BaseController
    {

        [HttpGet]
        [Authorize]
        public ActionResult Settings()
        {
            var settings = DB.UserSettings.FirstOrDefault(x => x.UserID == CurrentUser.ID);
            if (settings == null)
            {
                settings = new UserSetting()
                {
                    UserID = CurrentUser.ID,
                    OfferFriendship = 0,
                    InviteForMeeting = 0,
                    SendLetters = 0,
                    ViewFriends = 0,
                    ViewMeetings = 0,
                    ViewPhotos = 0
                };
                DB.UserSettings.InsertOnSubmit(settings);
                DB.SubmitChanges();
            }
            return View(settings);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Settings(UserSetting setting)
        {
            setting.IsPost = true;
            if (setting.NewPass.IsFilled() && setting.OldPass.IsFilled())
            {
                if (setting.NewPass.Length < 6)
                {
                    setting.Error = "Минимальная длина пароля - 6 символов";
                    return View(setting);
                }
                bool res = WebSecurity.ChangePassword(CurrentUser.Name, setting.OldPass, setting.NewPass);
                if (!res)
                {
                    setting.Error = "Текущий пароль указан неверно";
                    return View(setting);
                }
            }
            if (setting.OldMail.IsFilled() && setting.NewMail.IsFilled() && setting.NewMail.IsMailAdress())
            {
                if (CurrentUser.Email != setting.OldMail)
                {
                    setting.Error = "Текущий email указан неверно";
                    return View(setting);
                }
                var user = DB.Users.First(x => x.ID == CurrentUser.ID);
                user.Email = setting.NewMail;
                DB.SubmitChanges();
            }

            var dbs = DB.UserSettings.FirstOrDefault(x => x.UserID == CurrentUser.ID);
            if (dbs == null)
            {
                dbs = new UserSetting()
                {
                    UserID = CurrentUser.ID,
                    OfferFriendship = 0,
                    InviteForMeeting = 0,
                    SendLetters = 0,
                    ViewFriends = 0,
                    ViewMeetings = 0,
                    ViewPhotos = 0
                };
                DB.UserSettings.InsertOnSubmit(dbs);

            }

            dbs.LoadPossibleProperties(setting, new[] { "UserID" });
            DB.SubmitChanges();

            setting.Error = "Данные успешно сохранены";
            return View(setting);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Refill()
        {
            return View(CurrentUser);
        }
        [HttpPost]
        [Authorize]
        public ActionResult Refill(decimal? sum)
        {
            if (!sum.HasValue || sum.Value <= 0)
            {
                ViewBag.Error = true;
                return View(CurrentUser);
            }

            var sDesc = "Пополнение счета на сайте знакомств CityLove";
            var sOutSum = sum.Value.ToString("F", new System.Globalization.CultureInfo("en-US"));
            var sShpItem = "2";
            var sCulture = "ru";
            var sEncoding = "utf-8";

            var transaction = new MoneyTransaction()
            {
                Date = DateTime.Now,
                Status = 0,
                Sum = sum.Value,
                UserID = CurrentUser.ID,
                Description = "Пополнение счета"
            };

            DB.MoneyTransactions.InsertOnSubmit(transaction);
            DB.SubmitChanges();
            var nInvId = transaction.ID;
            var sCrcBase = string.Format("{0}:{1}:{2}:{3}"/*:shpItem={4}*/, Config.RoboLogin, sOutSum, nInvId, Config.RoboPass1/*, sShpItem*/);

            var md5 = new MD5CryptoServiceProvider();
            var buf = md5.ComputeHash(Encoding.ASCII.GetBytes(sCrcBase));
            var sCrc = buf.Select(b => string.Format("{0:x2}", b)).Aggregate((acc, el) => acc + el);

            string url = HttpUtility.UrlPathEncode(Config.RoboServer + "?" + "MrchLogin=" + Config.RoboLogin +
                         "&OutSum=" + sOutSum + "&InvId=" + nInvId + "&SignatureValue=" +
                         sCrc + "&Desc=" + sDesc);

            return Redirect(url);



        }
        public ActionResult Account()
        {
            return PartialView();
        }


        [Authorize]
        public ActionResult BlackList()
        {
            var list =
                DB.UserLocks.Where(x => x.UserID == CurrentUser.ID)
                    .Select(x => DB.Users.FirstOrDefault(z => z.ID == x.TargetID))
                    .Where(x => x != null);
            return View(list.ToList());
        }


        [Authorize]
        public ActionResult LockUser(int Target)
        {
            var exist = DB.UserLocks.FirstOrDefault(x => x.UserID == CurrentUser.ID && x.TargetID == Target);
            if (exist != null)
            {
                DB.UserLocks.DeleteOnSubmit(exist);
                DB.SubmitChanges();
                return RedirectToAction("Index", new { ID = Target, afterBlackList = 0 });

            }
            else
            {
                DB.UserLocks.InsertOnSubmit(new UserLock() { UserID = CurrentUser.ID, TargetID = Target });
                DB.SubmitChanges();
                return RedirectToAction("Index", new { ID = Target, afterBlackList = 1 });
            }


        }

        public ActionResult Index(int? ID, int? afterBlackList, int? fromMessage, int? fromFriends, int? fromDated, int? fromMeet)
        {
            User user = null;
            if (ID.HasValue)
            {
                user = DB.Users.FirstOrDefault(x => x.ID == ID);
                if (user != null && CurrentUser != null)
                {
                    var d = user.DatingsFromUser.FirstOrDefault(x => x.ToUserID == CurrentUser.ID);
                    if (d != null)
                    {
                        d.IsRead = true;
                        DB.SubmitChanges();
                    }

                    if (!user.IsHidden)
                    {
                        DB.Visits.InsertOnSubmit(new Visit()
                        {
                            VisitDate = DateTime.Now,
                            TargetUserID = user.ID,
                            VisitorID = CurrentUser.ID
                        });
                        DB.SubmitChanges();
                    }
                }
            }
            if (user == null && CurrentUser != null)
            {
                user = CurrentUser;
            }




            if (afterBlackList.HasValue)
            {
                ViewBag.AfterBlackList = afterBlackList;
            }
            if (fromMessage.HasValue)
            {
                ViewBag.AfterMessage = 1;
            }
            if (fromFriends.HasValue)
            {
                ViewBag.AfterFriend = 1;
            }
            if (fromDated.HasValue)
            {
                ViewBag.AfterDated = 1;
            }
            if (fromMeet.HasValue)
            {
                ViewBag.AfterMeet = 1;
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult AllPhoto(int? ID)
        {

            if (!ID.HasValue)
            {
                if (CurrentUser != null)
                {
                    ID = CurrentUser.ID;
                }
            }
            if (!ID.HasValue)
            {
                return RedirectToAction("NotFound", "Home");
            }
            ViewBag.User = DB.Users.FirstOrDefault(x => x.ID == ID);
            var photos = DB.UserPhotos.Where(x => x.UserID == ID);
            return View(photos.ToList());
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase file1, HttpPostedFileBase file2, HttpPostedFileBase file3, HttpPostedFileBase file4, HttpPostedFileBase file5, HttpPostedFileBase file6)
        {
            var files = new[] { file1, file2, file3, file4, file5, file6 }.Where(x => x != null && x.ContentLength > 0);
            foreach (HttpPostedFileBase file in files)
            {
                if (file.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string path = "/Content/Avatars/" + fileName;
                    file.SaveAs(Server.MapPath(path));

                    DB.UserPhotos.InsertOnSubmit(new UserPhoto() { Path = path, UserID = CurrentUser.ID });
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("Index", "Cabinet");
        }


        public ActionResult LeftMenu()
        {
            return PartialView();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProfileEdit()
        {
            return View(CurrentUser);
        }
        [HttpPost]
        [Authorize]
        public ActionResult ProfileEdit(User user)
        {
            user.IsPost = true;
            DateTime date = DateTime.MinValue;
            try
            {
                date = new DateTime(user.Year.Value, user.Month.Value, user.Day.Value);
                user.BirthDate = date;
            }
            catch
            {
                user.Year = null;
                user.Month = null;
                user.Day = null;
                user.InitAddFieldsFromMain();
                return View(user);
            }

            //CurrentUser.LoadPossibleProperties(new[] { "ID", "TownID" });
            if (user.UserName.IsNullOrEmpty())
            {
                user.InitAddFieldsFromMain();
                return View(user);
            }

            user.SaveFiledsToMain();

            var dbu = DB.Users.FirstOrDefault(x => x.ID == user.ID);
            if (dbu != null)
            {
                dbu.LoadPossibleProperties(user, new[] { "ID", "TownID", "Name", "Email" });
            }
            DB.SubmitChanges();

            CurrentUser = null;

            return View(CurrentUser);
        }

        [Authorize]
        public ActionResult RemoveFromList(int ID)
        {
            var ul = DB.UserLocks.FirstOrDefault(x => x.UserID == CurrentUser.ID && x.TargetID == ID);
            if (ul != null)
            {
                DB.UserLocks.DeleteOnSubmit(ul);
            }
            DB.SubmitChanges();
            return RedirectToAction("BlackList");

        }

        [Authorize]
        public ActionResult DeleteMsg(int ID)
        {
            var msg = DB.Messages.FirstOrDefault(x => x.ID == ID);
            if (msg != null)
            {
                DB.Messages.DeleteOnSubmit(msg);
                DB.SubmitChanges();
            }
            return new ContentResult();
        }

        [Authorize]
        public ActionResult Messages(bool? afterAnswer)
        {
            ViewBag.AfterAnswer = afterAnswer;
            var list = DB.Messages.Where(x => x.FromUserID == CurrentUser.ID || x.ToUserID == CurrentUser.ID).ToList();
            List<Message> readList = new List<Message>();
            foreach (var message in list.Where(x => x.ToUserID == CurrentUser.ID && !x.IsRead))
            {
                message.IsRead = true;
                readList.Add(message);
            }
            DB.SubmitChanges();

            foreach (var message in readList)
            {
                message.IsRead = false;
            }

            return View(list);
        }

        [Authorize]
        public ActionResult WriteMessage(string Message, int TargetID)
        {
            DB.Messages.InsertOnSubmit(new Message()
            {
                CreateDate = DateTime.Now,
                IsRead = false,
                Text = Message.ClearHTML(),
                ToUserID = TargetID,
                FromUserID = CurrentUser.ID
            });
            DB.SubmitChanges();
            return RedirectToAction("Index", new { fromMessage = 1, ID = TargetID });
        }

        [Authorize]
        public ActionResult ToFriends(int TargetID)
        {
            var target = DB.Users.FirstOrDefault(x => x.ID == TargetID);
            if (target != null)
            {
                if (!CurrentUser.IsFriends(target))
                {
                    DB.Friends.InsertOnSubmit(new Friend()
                    {
                        CreateDate = DateTime.Now,
                        FromUserID = CurrentUser.ID,
                        ToUserID = TargetID,
                        Status = 0
                    });
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("Index", new { ID = TargetID, fromFriends = 1 });
        }

        [Authorize]
        public ActionResult FriendsIndex()
        {
            return View();
        }
        [Authorize]
        public ActionResult Friends(string Search, int Order = 0)
        {
            var friendship = new Friendship(DB, Search, Order, CurrentUser);
            return PartialView(friendship);
        }

        [Authorize]
        public ActionResult FriendAccept(int ID, int Target)
        {
            var friend = DB.Friends.FirstOrDefault(x => x.ID == ID);
            if (friend != null)
            {
                friend.Status = Target;
                friend.IsRead = true;
                DB.SubmitChanges();
            }
            return RedirectToAction("FriendsIndex");
        }

        [Authorize]
        public ActionResult ToDated(int TargetID)
        {
            var target = DB.Users.FirstOrDefault(x => x.ID == TargetID);
            if (target != null)
            {
                if (!CurrentUser.IsDated(target))
                {
                    DB.Datings.InsertOnSubmit(new Dating()
                    {
                        CreateDate = DateTime.Now,
                        FromUserID = CurrentUser.ID,
                        ToUserID = TargetID,
                        Status = 0,
                        IsRead = false
                    });
                    DB.SubmitChanges();
                }
            }
            return RedirectToAction("Index", new { ID = TargetID, fromDated = 1 });
        }

        [Authorize]
        public ActionResult Datings()
        {
            return View(new MyDatings(DB, CurrentUser));
        }

        public ActionResult DatingAccept(int ID, int Target)
        {
            var friend = DB.Datings.FirstOrDefault(x => x.ID == ID);
            if (friend != null)
            {
                friend.Status = Target;
                friend.IsRead = true;
                DB.SubmitChanges();
            }
            return RedirectToAction("Datings");
        }

        [Authorize]
        public ActionResult MeetUser(int MeetID, int TargetUserID)
        {
            var meet = DB.Meetings.FirstOrDefault(x => x.Author == CurrentUser.ID && x.ID == MeetID);
            if (meet != null)
            {
                var mp = DB.MeetingPeoples.FirstOrDefault(x => x.UserID == TargetUserID && x.MeetingID == MeetID);
                if (mp == null)
                {
                    mp = new MeetingPeople()
                    {
                        MeetingID = MeetID,
                        Status = 0,
                        UserID = TargetUserID,
                        SendDate = DateTime.Now
                    };
                    DB.MeetingPeoples.InsertOnSubmit(mp);
                    DB.SubmitChanges();
                    return RedirectToAction("Index", new { ID = TargetUserID, fromMeet = 1 });
                }
            }
            return RedirectToAction("Index", new { ID = TargetUserID });
        }


        [Authorize]
        [HttpGet]
        public ActionResult Buy()
        {
            return View(CurrentUser);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Buy(decimal cost, string type, TimeSpan duration, int? arg)
        {

            if (CurrentUser.Balance < cost)
            {
                return new ContentResult();
            }

            var date = DateTime.Now.Date;
            var user = DB.Users.First(x => x.ID == CurrentUser.ID);
            switch (type)
            {
                case "hidden":
                    if (CurrentUser.HiddenEnd.HasValue && CurrentUser.HiddenEnd.Value.Date > DateTime.Now.Date)
                        date = CurrentUser.HiddenEnd.Value.Date;
                    user.HiddenEnd = date.Add(duration);
                    user.IsHidden = true;
                    break;
                case "up":
                    if (CurrentUser.UpEnd.HasValue && CurrentUser.UpEnd.Value.Date > DateTime.Now.Date)
                        date = CurrentUser.UpEnd.Value.Date;
                    user.UpEnd = date.Add(duration);
                    user.IsUp = true;
                    break;
                case "vip":
                    if (CurrentUser.VipEnd.HasValue && CurrentUser.VipEnd.Value.Date > DateTime.Now.Date)
                        date = CurrentUser.VipEnd.Value.Date;
                    user.VipEnd = date.Add(duration);
                    user.IsVip = true;
                    break;
                case "place":
                    var place = DB.Places.FirstOrDefault(x => x.ID == arg);
                    if (place != null)
                    {
                        if (place.VipEnd.HasValue && place.VipEnd.Value.Date > DateTime.Now.Date)
                            date = place.VipEnd.Value.Date;
                        place.VipEnd = date.Add(duration);
                        place.IsVip = true;
                    }
                    break;

            }

            var transaction = new MoneyTransaction()
            {
                Date = DateTime.Now,
                Description = type,
                Status = 1,
                Sum = cost * -1,
                UserID = CurrentUser.ID
            };

            DB.MoneyTransactions.InsertOnSubmit(transaction);


            DB.SubmitChanges();

            return new ContentResult() { Content = "Vip-опция успешно подключена" };

        }


        public ActionResult VisitsList(int TargetID)
        {
            var list = new List<Visit>();
            var target = DB.Users.FirstOrDefault(x => x.ID == TargetID);
            if (target != null && CurrentUser!=null && !target.IsHidden && target.ID == CurrentUser.ID)
            {
                list =
                    DB.Visits.Where(x => x.TargetUserID == TargetID && x.VisitorID != TargetID)
                        .GroupBy(x => x.VisitorID)
                        .Select(x => x.OrderByDescending(z => z.VisitDate).FirstOrDefault())
                        .Where(x => x != null)
                        .Take(50)
                        .ToList();
            }

            return PartialView(list);
        }

        public ActionResult CreateAvatar(int id)
        {
            var photo = DB.UserPhotos.FirstOrDefault(x => x.ID == id);
            if (photo != null)
            {
                var photos = DB.UserPhotos.Where(x => x.UserID == photo.UserID);
                foreach (var userPhoto in photos)
                {
                    userPhoto.IsAvatar = false;
                }
                photo.IsAvatar = true;
                DB.SubmitChanges();
                return RedirectToAction("AllPhoto", new { ID = photo.UserID });

            }
            return Redirect("/");
        }

        public ActionResult DeletePhoto(int id)
        {
            var photo = DB.UserPhotos.FirstOrDefault(x => x.ID == id);
            if (photo != null)
            {
                var uid = photo.UserID;
                DB.UserPhotos.DeleteOnSubmit(photo);
                DB.SubmitChanges();
                return RedirectToAction("AllPhoto", new { ID = uid });
            }
            return Redirect("/");

        }

        [Authorize]
        public ActionResult WriteMessageAnswer(string Message, int TargetID)
        {
            DB.Messages.InsertOnSubmit(new Message()
            {
                CreateDate = DateTime.Now,
                IsRead = false,
                Text = Message.ClearHTML(),
                ToUserID = TargetID,
                FromUserID = CurrentUser.ID
            });
            DB.SubmitChanges();
            return RedirectToAction("Messages", new { afterAnswer = true});
        }
    }
}
