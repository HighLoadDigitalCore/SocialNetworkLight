using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SexiLove.Extensions;
using SexiLove.Extensions.Mail;
using SexiLove.Models;
using WebMatrix.WebData;

namespace SexiLove.Controllers
{
    [Authorize(Roles = "Admin, Moderator")]
    public class AdminController : BaseController
    {

        public ActionResult Towns()
        {
            return View(DB.Towns.ToList());
        }

        [HttpGet]
        public ActionResult EditTown(int? id)
        {
            var town = DB.Towns.FirstOrDefault(x => x.ID == (id ?? 0));
            if (town == null)
                town = new Town();

            return View(town);
        }
        [HttpPost]
        public ActionResult EditTown(Town town)
        {
            town.IsPost = true;
            if (town.Name.IsNullOrEmpty())
                return View(town);
            if (town.ID > 0)
            {
                var dt = DB.Towns.FirstOrDefault(x => x.ID == town.ID);
                dt.LoadPossibleProperties(town);
                DB.SubmitChanges();
            }
            else
            {
                DB.Towns.InsertOnSubmit(town);
                DB.SubmitChanges();
            }

            return RedirectToAction("Towns");
        }

        public ActionResult DeleteTown(int id)
        {
            var town = DB.Towns.FirstOrDefault(x => x.ID == id);
            if (town != null)
            {
                DB.Towns.DeleteOnSubmit(town);
                DB.SubmitChanges();
            }
            return RedirectToAction("Towns");
        }

        [HttpGet]
        public ActionResult EditUser(int id)
        {
            var town = DB.BlockedUsers.FirstOrDefault(x => x.UserID == id) ?? new BlockedUser()
            {
                BlockType = -1,
                UserID = id,
                User = DB.Users.First(x=> x.ID == id)
            };

            return View(town);
        }


        [HttpPost]
        public ActionResult EditUser(BlockedUser user)
        {
            if (user.ID > 0)
            {
                var u = DB.BlockedUsers.FirstOrDefault(x => x.ID == user.ID);
                if (u == null)
                {
                    DB.BlockedUsers.InsertOnSubmit(user);
                }
                else
                {
                    u.LoadPossibleProperties(user);
                }
            }
            else
            {
                DB.BlockedUsers.InsertOnSubmit(user);
            }
            DB.SubmitChanges();
            return RedirectToAction("Users");
        }

        public ActionResult Users()
        {
            return View(DB.Users.ToList());
        }

        public ActionResult AuthUser(int id)
        {
            var user = DB.Users.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.Name, false);
            }
            else
            {
                return RedirectToAction("Users");
            }
            return RedirectToAction("Index", "Cabinet");
        }

        [HttpGet]
        public ActionResult EditUserPass(int ID)
        {
            ViewBag.ID = ID;
            return View();
        }


        [HttpPost]
        public ActionResult EditUserPass(int ID, string Password)
        {
            ViewBag.ID = ID;
            if (Password.IsFilled())
            {
                var user = DB.Users.FirstOrDefault(x => x.ID == ID);
                if (user != null)
                {
                    WebSecurity.ResetPassword(WebSecurity.GeneratePasswordResetToken(user.Name), Password);
                }
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public ActionResult EditTextPage(int id)
        {
            var page = DB.TextPages.FirstOrDefault(x => x.ID == id);
            if (page == null)
                return RedirectToAction("TextPages");
            return View(page);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTextPage(TextPage p)
        {
            var page = DB.TextPages.FirstOrDefault(x => x.ID == p.ID);
            if (page == null)
                return RedirectToAction("TextPages");

            page.LoadPossibleProperties(p, new[] { "URL" });
            if (page.Text.IsNullOrEmpty())
                page.Text = "";
            if (page.Name.IsNullOrEmpty())
                page.Name = "";
            DB.SubmitChanges();
            return RedirectToAction("TextPages");
        }

        public ActionResult TextPages()
        {
            return View(DB.TextPages.ToList());
        }


        public ActionResult Places()
        {
            return View(DB.Places.ToList());
        }
        public ActionResult PlacesModer()
        {
            return View(DB.Places.Where(x => x.PlaceAdmins.Any(z => z.UserID == CurrentUser.ID)).ToList());
        }

        public ActionResult PlaceImageDelete(int id)
        {
            var img = DB.PlaceImages.FirstOrDefault(x => x.ID == id);
            int pid = 0;
            if (img != null)
            {
                pid = img.PlaceID;
                DB.PlaceImages.DeleteOnSubmit(img);
                DB.SubmitChanges();
            }
            if (pid > 0)
                return RedirectToAction("EditPlace", new { ID = pid });
            else return RedirectToAction("Places");
        }

        public ActionResult DeletePlace(int id)
        {
            var place = DB.Places.FirstOrDefault(x => x.ID == id);
            if (place != null)
            {
                DB.Places.DeleteOnSubmit(place);
                DB.SubmitChanges();
            }
            return RedirectToAction("Places");
        }



        [HttpGet]
        public ActionResult EditPlace(int? id, int? fromSave)
        {
            if (fromSave.HasValue)
            {
                ViewBag.Message = "Данные успешно сохранены";
            }
            var town = DB.Places.FirstOrDefault(x => x.ID == id) ?? new Place();
            return View(town);
        }

        [HttpGet]
        public ActionResult EditPlaceModer(int? id, int? fromSave)
        {
            if (fromSave.HasValue)
            {
                ViewBag.Message = "Данные успешно сохранены";
            }
            var town = DB.Places.FirstOrDefault(x => x.ID == id) ?? new Place();
            return View(town);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditPlace(Place place, HttpPostedFileBase Logo, HttpPostedFileBase Image, FormCollection collection)
        {
            if (collection["Save"].IsFilled())
            {

                bool needCreateModer = false;
                place.IsPost = true;
                if (/*Logo == null || Logo.ContentLength == 0 || Image == null || Image.ContentLength == 0 ||*/
                    place.Name.IsNullOrEmpty() || place.Adress.IsNullOrEmpty() || place.Description.IsNullOrEmpty() ||
                    !place.MinPay.HasValue || !place.MaxPay.HasValue ||
                    place.Type.IsNullOrEmpty() || (place.WishToBeAdmin && !place.AdminMail.IsMailAdress()))
                    return View(place);

                if (place.ID > 0)
                {

                    var p = DB.Places.First(x => x.ID == place.ID);
                    needCreateModer = p.AdminMail != place.AdminMail && place.AdminMail.IsMailAdress() && place.WishToBeAdmin ;
                    p.LoadPossibleProperties(place, new[] { "Logo", "Approved" });
                    place = p;
                    if (!needCreateModer)
                        place.AdminMail = "";
                }


                if (!(Logo == null || Logo.ContentLength == 0))
                {
                    string fileNameLogo = Guid.NewGuid() + Path.GetExtension(Logo.FileName);
                    string pathLogo = "/Content/Places/" + fileNameLogo;
                    Logo.SaveAs(Server.MapPath(pathLogo));
                    place.Logo = pathLogo;
                }
                if (!(Image == null || Image.ContentLength == 0))
                {
                    string fileNameImage = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                    string pathImage = "/Content/Places/" + fileNameImage;
                    Image.SaveAs(Server.MapPath(pathImage));
                    DB.PlaceImages.InsertOnSubmit(new PlaceImage() { Path = pathImage, Place = place });


                }
                var isNew = false;
                if (place.ID == 0)
                {
                    isNew = true;
                    needCreateModer = place.AdminMail.IsMailAdress() && place.WishToBeAdmin;
                    DB.Places.InsertOnSubmit(place);
                }

                if (needCreateModer)
                {
                    var u = DB.Users.FirstOrDefault(x => x.Email.ToLower().Trim() == place.AdminMail.ToLower().Trim());
                    if (u == null)
                    {
                        var dict = new Dictionary<string, object>();

                        var pass = new Random(DateTime.Now.Millisecond).GeneratePassword();
                        dict.Add("UserName", place.AdminMail);
                        dict.Add("Email", place.AdminMail);
                        MembershipProvider.CreateUserAndAccount(place.AdminMail, pass, false, dict);
                        RoleProvider.AddUsersToRoles(new[] {place.AdminMail}, new[] {"Client", "Moderator"});
                        NotifyMail.SendNotify("PlaceAdminCreate", place.AdminMail,
                            format => string.Format(format, HostName),
                            format => string.Format(format, place.AdminMail, pass, HostName)
                            );

                        u = DB.Users.First(x => x.Email.ToLower().Trim() == place.AdminMail.ToLower().Trim());

                        var exist = DB.PlaceAdmins.FirstOrDefault(x => x.PlaceID == place.ID && x.UserID == u.ID);
                        if (exist == null)
                        {
                            exist = new PlaceAdmin() {PlaceID = place.ID, UserID = u.ID};
                            DB.PlaceAdmins.InsertOnSubmit(exist);
                        }

                    }
                    else
                    {
                        NotifyMail.SendNotify("PlaceAdminExist", place.AdminMail,
                            format => string.Format(format, HostName),
                            format => string.Format(format, "", "", HostName)
                            );


                        if (u.webpages_UsersInRoles.All(x => x.webpages_Role.RoleName != "Moderator"))
                        {
                            RoleProvider.AddUsersToRoles(new[] {u.Name}, new[] {"Moderator"});
                        }

                        var exist = DB.PlaceAdmins.FirstOrDefault(x => x.PlaceID == place.ID && x.UserID == u.ID);
                        if (exist == null)
                        {
                            exist = new PlaceAdmin() {PlaceID = place.ID, UserID = u.ID};
                            DB.PlaceAdmins.InsertOnSubmit(exist);
                        }

                    }
                }
                else if(place.PlaceAdmins.Any())
                {
                    
                    DB.PlaceAdmins.DeleteAllOnSubmit(place.PlaceAdmins);
                }

                DB.SubmitChanges();
                return isNew ? RedirectToAction("Places") : RedirectToAction("EditPlace", new { ID = place.ID, FromSave = 1 });
            }
            else
            {
                if (place.ID == 0)
                    return RedirectToAction("Places");
                var p = DB.Places.First(x => x.ID == place.ID);
                if (!p.Approved)
                {
                    p.Approved = true;

                    if (p.AdminMail.IsMailAdress())
                    {
                        var user = DB.Users.FirstOrDefault(x => x.Email.ToLower().Trim() == p.AdminMail.ToLower().Trim());
                        if (user == null)
                        {
                            var dict = new Dictionary<string, object>();

                            var pass = new Random(DateTime.Now.Millisecond).GeneratePassword();
                            dict.Add("UserName", p.AdminMail);
                            dict.Add("Email", p.AdminMail);
                            MembershipProvider.CreateUserAndAccount(p.AdminMail, pass, false, dict);
                            RoleProvider.AddUsersToRoles(new[] { p.AdminMail }, new[] { "Client", "Moderator" });
                            NotifyMail.SendNotify("PlaceAdminCreate", p.AdminMail,
                                format => string.Format(format, HostName),
                                format => string.Format(format, p.AdminMail, pass, HostName)
                                );

                            user = DB.Users.First(x => x.Email.ToLower().Trim() == p.AdminMail.ToLower().Trim());

                            var exist = DB.PlaceAdmins.FirstOrDefault(x => x.PlaceID == p.ID && x.UserID == user.ID);
                            if (exist == null)
                            {
                                exist = new PlaceAdmin() { PlaceID = p.ID, UserID = user.ID };
                                DB.PlaceAdmins.InsertOnSubmit(exist);
                            }

                        }
                        else
                        {
                            NotifyMail.SendNotify("PlaceAdminExist", p.AdminMail,
                                format => string.Format(format, HostName),
                                format => string.Format(format, "", "", HostName)
                                );


                            if (user.webpages_UsersInRoles.All(x => x.webpages_Role.RoleName != "Moderator"))
                            {
                                RoleProvider.AddUsersToRoles(new[] { user.Name }, new[] { "Moderator" });
                            }

                            var exist = DB.PlaceAdmins.FirstOrDefault(x => x.PlaceID == p.ID && x.UserID == user.ID);
                            if (exist == null)
                            {
                                exist = new PlaceAdmin() { PlaceID = p.ID, UserID = user.ID };
                                DB.PlaceAdmins.InsertOnSubmit(exist);
                            }

                        }



                    }
                }
                else
                {
                    p.Approved = false;
                    if (p.AdminMail.IsMailAdress())
                    {
                        NotifyMail.SendNotify("PlaceAdminBlock", p.AdminMail,
                               format => string.Format(format, HostName),
                               format => string.Format(format, "", "", HostName, p.Name)
                               );
                    }
                }
                DB.SubmitChanges();
                return RedirectToAction("Places");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditPlaceModer(Place place, HttpPostedFileBase Logo, HttpPostedFileBase Image, FormCollection collection)
        {
            if (collection["Save"].IsFilled())
            {

                place.IsPost = true;
                if (/*Logo == null || Logo.ContentLength == 0 || Image == null || Image.ContentLength == 0 ||*/
                    place.Name.IsNullOrEmpty() || place.Adress.IsNullOrEmpty() || place.Description.IsNullOrEmpty() ||
                    !place.MinPay.HasValue || !place.MaxPay.HasValue ||
                    place.Type.IsNullOrEmpty() || (place.WishToBeAdmin && !place.AdminMail.IsMailAdress()))
                    return View(place);
                
                if (place.ID > 0)
                {
                    var p = DB.Places.First(x => x.ID == place.ID);
                    p.LoadPossibleProperties(place, new[] { "Logo", "Approved" });
                    p.Approved = true;
                    place = p;
                }


                if (!(Logo == null || Logo.ContentLength == 0))
                {
                    string fileNameLogo = Guid.NewGuid() + Path.GetExtension(Logo.FileName);
                    string pathLogo = "/Content/Places/" + fileNameLogo;
                    Logo.SaveAs(Server.MapPath(pathLogo));
                    place.Logo = pathLogo;
                }
                if (!(Image == null || Image.ContentLength == 0))
                {
                    string fileNameImage = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                    string pathImage = "/Content/Places/" + fileNameImage;
                    Image.SaveAs(Server.MapPath(pathImage));
                    DB.PlaceImages.InsertOnSubmit(new PlaceImage() { Path = pathImage, Place = place });


                }
                if (place.ID > 0)
                {
                    DB.SubmitChanges();
                }

                return RedirectToAction("EditPlaceModer", new { ID = place.ID, FromSave = 1 });
            }
            else
            {
            /*    if (place.ID == 0)
                    return RedirectToAction("PlacesModer");
                var p = DB.Places.First(x => x.ID == place.ID);
                p.Approved = !p.Approved;
                DB.SubmitChanges();*/
                return RedirectToAction("PlacesModer");
            }
        }

        public ActionResult Transactions(int ID)
        {
            var user = DB.Users.FirstOrDefault(x => x.ID == ID);
            return View(user);
        }

        public ActionResult Qsts()
        {
            return View(DB.FAQs.ToList());
        }


        [HttpGet]
        public ActionResult EditQsts(int? ID)
        {
            var qst = DB.FAQs.FirstOrDefault(x => x.ID == (ID ?? 0)) ?? new FAQ()
            {
                Visible = true,
                UserID = CurrentUser.ID,
                CreateDate = DateTime.Now
            };
            return View(qst);
        }


        [HttpPost]
        public ActionResult EditQsts(FormCollection collection)
        {
            
            
            int id = (int) collection.GetValue("ID").ConvertTo(typeof (int));
            FAQ qst = new FAQ();
            if (id > 0)
            {
                qst = DB.FAQs.First(x => x.ID == id);
            }
            qst.IsPost = true;
            string q = (string) collection.GetValue("Qst").ConvertTo(typeof (string));
            string a = (string) collection.GetValue("Ans").ConvertTo(typeof (string));
            bool v = (bool) collection.GetValue("Visible").ConvertTo(typeof (bool));
            DateTime c = (DateTime)collection.GetValue("CreateDate").ConvertTo(typeof(DateTime));

            qst.Qst = q;
            qst.Ans = a;
            qst.Visible = v;
            qst.CreateDate = c;

            if (id == 0)
            {
                qst.UserID = CurrentUser.ID;
            }

            if (qst.Qst.IsNullOrEmpty() || qst.Ans.IsNullOrEmpty())
                return View(qst);

            if (id == 0)
            {
                DB.FAQs.InsertOnSubmit(qst);
            }
            DB.SubmitChanges();

            return RedirectToAction("Qsts");
        }

        public ActionResult DeleteQst(int id)
        {
            var qst = DB.FAQs.FirstOrDefault(x => x.ID == id);
            if (qst != null)
            {
                DB.FAQs.DeleteOnSubmit(qst);
                DB.SubmitChanges();
            }
            return RedirectToAction("Qsts");
        }

        [HttpGet]
        public ActionResult Event()
        {
            return View();
        }   
        
        [HttpPost]
        public ActionResult Event(FormCollection collection)
        {
            ViewBag.Message = "Список мероприятий успешно обновлен";
            var parser = new Parser();
            parser.Parse();
            return View();
        }

        [AllowAnonymous]
        public ActionResult Exit()
        {
            WebSecurity.Logout();
            return Redirect("/");
        }
    }
}
