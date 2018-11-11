using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using SexiLove.Extensions;
using SexiLove.Extensions.Mail;
using SexiLove.Models;
using WebMatrix.WebData;

namespace SexiLove.Controllers
{
    public class ProfileController : BaseController
    {


        [HttpGet]
        public ActionResult Restore()
        {
            var model = new LoginModel();
            return PartialView(model);
        }


        [HttpPost]
        public ActionResult Restore(LoginModel model)
        {
            if (model.Email.IsNullOrEmpty())
            {
                model.Message = "Неверно указан e-mail";
            }
            else
            {
                var user =
                    DB.Users.FirstOrDefault(
                        x => x.Email.ToLower() == model.Email.ToLower() || x.Name.ToLower() == model.Email.ToLower());
                if (user == null)
                {
                    model.Message = "Неверно указан e-mail";
                }
                else
                {
                    var newPass = new Random(DateTime.Now.Millisecond).GeneratePassword();
                    var token = WebSecurity.GeneratePasswordResetToken(user.Name);
                    WebSecurity.ResetPassword(token, newPass);
                    NotifyMail.SendNotify("ForgotPassword", user.Email,
                                      format => string.Format(format, HostName),
                                      format => string.Format(format, user.Email, newPass, HostName));

                    model.Message = "Новый пароль отправлен на ваш E-mail";
                }
            }
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Login()
        {

            var model = new LoginModel();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (model.Email.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
            {
                model.Message = "Неверно указан логин или пароль";
                return PartialView(model);
            }



            string addr = HttpContext.Request.UserHostAddress;
            long? intAddress = addr.ToIPInt();
            var retUser = DB.Users.FirstOrDefault(x => x.Email.ToLower() == model.Email.ToLower() || x.Name.ToLower() == model.Email.ToLower());
            if (retUser != null)
            {
                var block = retUser.BlockedUsers.FirstOrDefault();
                if (block != null && block.BlockType == 0)
                {

                    if (block.BlockTill.HasValue && block.BlockTill.Value > DateTime.Now)
                    {
                        block.StartIP = intAddress;
                        block.EndIP = intAddress;
                        DB.SubmitChanges();

                        model.Message = "Вы заблокированы администратором сайта";
                    }
                }
                else if (block != null && block.BlockType == 1)
                {
                    if (block.BlockTill.HasValue && block.BlockTill.Value > DateTime.Now)
                    {
                        if (block.StartIP.HasValue && block.EndIP.HasValue)
                        {
                            if (intAddress.HasValue && block.StartIP.Value <= intAddress &&
                                intAddress <= block.EndIP.Value)
                            {
                                model.Message = "Вы заблокированы администратором сайта";
                            }
                        }
                    }
                }
                else
                {
                    if (WebSecurity.Login(model.Email, model.Password, model.Remember))
                    {
                        model.RedirectURL = Url.Action("Index", "Cabinet");
                    }
                    else
                    {
                        model.Message = "Неверно указан логин или пароль";
                    }
                    if (block != null)
                    {
                        block.StartIP = intAddress;
                        block.EndIP = intAddress;
                        DB.SubmitChanges();
                    }


                }
            }
            else
            {
                model.Message = "Такого пользователя не существует";
            }


            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterModel());
        }


        [HttpPost]
        public ActionResult Register(RegisterModel model, HttpPostedFileBase Photo)
        {
            model.IsPost = true;
            if (Photo != null && Photo.ContentLength > 0)
            {
                model.Photo = Photo.FileName;
            }

            if (model.Email.IsFilled())
            {
                var exist = DB.Users.Where(x => x.Name.ToLower() == model.Email.ToLower() || x.Email.ToLower() == model.Email.ToLower());
                if (exist.Any())
                {
                    model.Email = null;
                }
            }

            if (!model.Day.HasValue || !model.Month.HasValue || !model.Year.HasValue || !model.UserName.IsFilled() || !model.Town.HasValue || !model.Email.IsFilled() || !model.Email.IsMailAdress() || model.Password.IsNullOrEmpty() /*|| Photo == null || Photo.ContentLength == 0*/)
                return View(model);

            DateTime date;
            try
            {
                date = new DateTime(model.Year.Value, model.Month.Value, model.Day.Value);
            }
            catch
            {
                model.Month = null;
                model.Day = null;
                return View(model);
            }

            /*var pass = new Random(DateTime.Now.Millisecond).GeneratePassword();*/
            var dict = new Dictionary<string, object>();
            dict.Add("BirthDate", date);
            dict.Add("TownID", model.Town);
            dict.Add("Email", model.Email);
            dict.Add("Sex", model.Sex);

            var name = "";
            var surname = "";
            var ar = model.UserName.Split<string>(" ");
            name = ar.ElementAt(0);
            if (ar.Count() > 1)
            {
                surname = ar.ElementAt(1);
            }

            dict.Add("UserName", name);
            dict.Add("UserSurname", surname);
            MembershipProvider.CreateUserAndAccount(model.Email, model.Password, false, dict);
            RoleProvider.AddUsersToRoles(new[] { model.Email }, new[] { "Client" });

            var user = DB.Users.FirstOrDefault(x => x.Email == model.Email);
            if (user != null && Photo != null && Photo.ContentLength != 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                string path = "/Content/Avatars/" + fileName;
                Photo.SaveAs(Server.MapPath(path));

                DB.UserPhotos.InsertOnSubmit(new UserPhoto() { Path = path, UserID = user.ID, IsAvatar = true});
                DB.SubmitChanges();
            }


            WebSecurity.Login(model.Email, model.Password, true);



            NotifyMail.SendNotify("Register", model.Email,
                format => string.Format(format, HostName),
                format => string.Format(format, model.Email, model.Password, HostName)
                );



            return RedirectToAction("Index", "Cabinet");


            /*
                        NotifyMail.SendNotify("ForgotPassword", user.Email,
                                        format => string.Format(format, HostName),
                                        format => string.Format(format, user.Email, user.Password, HostName));
            */

        }

        public ActionResult Socials()
        {
            var result = SocialAuthResult.CheckAuth();
            if (result.HasResult && result.User != null)
            {
                return RedirectToAction(result.IsNew ? "ProfileEdit" : "Index", "Cabinet");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult AuthInner()
        {
            return PartialView(new LoginModel());
        }
        [HttpPost]
        public ActionResult AuthInner(LoginModel model)
        {
            if (model.Email.IsNullOrEmpty() || model.Password.IsNullOrEmpty() || !WebSecurity.Login(model.Email, model.Password))
            {
                model.Message = "Неверно указан логин или пароль";

            }
            else
            {
                model.RedirectURL = Url.Action("Index", "Cabinet");
            }
            return PartialView(model);
        }

        public ActionResult LeftAuth()
        {
            return PartialView();
        }
    }
}
