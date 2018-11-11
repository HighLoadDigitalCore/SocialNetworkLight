using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using SexiLove.Extensions;
using WebMatrix.WebData;

namespace SexiLove.Models
{

    public partial class User
    {

        public int? MyAge
        {
            get
            {
                if (!BirthDate.HasValue)
                    return null;
                return (int)((DateTime.Now.Subtract(BirthDate.Value).TotalMinutes / 1440) / 365);
            }
        }

        public string ToPropList()
        {
            string[] keys = new[]
            {
                "Age", "Orientation", "Purpose", "SearchFor", "Relations", "HasChildren", "Sponsor", "Height", "Weight", "Body",
                "Appearence", "Eyes", "Hears", "Education",
                "Spec", "Job", "WorkType", "CompanyName", "Earn", "MonthSalary", "HasCar", "LivingConditions",
                "LivingTown", "Smoking", "Drinking", "Drugs", "LifePriority", "Music", "Sport", "Hobby", "Books",
                "Films", "GoodKinds", "BadKinds", "MainPurpose", "SexRole", "SexPeriod", "SexRelation",
                "SexRelationDetail", "AboutMe"
            };
            string[] names = new[]
            {
                "Возраст", "Ориентация", "Цель знакомства", "Кого я ищу", "Отношения", "Дети", "Спонсор", "Рост", "Вес",
                "Телосложение",
                "Тип внешности", "Цвет глаз", "Цвет волос", "Образование",
                "Специальность", "Сфера деятельности/профессия", "Тип работы", "Название компании", "Я зарабатываю",
                "Средний доход в месяц", "Автомобиль", "Жилищные условия",
                "Город проживания ваш родной город?", "Курение", "Алкоголь", "Наркотики", "Самое главное в жизни",
                "Какую музыку вы любите", "Занятия спортом", "Хобби", "Любимые книги",
                "Любимые фильмы", "Качества, которые цените в людях", "Качества, которые не принимаете в людях",
                "Главная цель в жизни", "Роль секса в вашей жизни", "Как часто вы бы хотели заниматься сексом?",
                "Отношение к нестандартному сексу",
                "Про нестандартный секс", "О себе"
            };

            var list = keys.Select((x, index) => new { Key = x, Name = names[index], Value = this.GetPropertyValue(x) })
                .Where(x => ((string)x.Value).IsFilled()).ToList();

            return list.Select(x => string.Format("<dt>{0}: </dt><dd>{1}</dd>", x.Name, x.Value.ToString().Replace(";", "; "))).JoinToString("");

        }

        public string Age
        {
            get
            {
                if (!BirthDate.HasValue)
                    return null;
                var age = MyAge ?? 0;
                int rem = 0;
                Math.DivRem(age, 10, out rem);

                string postfix = "";
                if (rem == 0 || rem >= 5)
                    postfix = "лет";
                else if (rem == 1)
                    postfix = "год";
                else if (rem > 1 && rem < 5)
                    postfix = "года";

                return age + " " + postfix;

            }
        }

        public string Purpose1 { get; set; }
        public string Purpose2 { get; set; }
        public string Purpose3 { get; set; }
        public string Purpose4 { get; set; }
        public string Purpose5 { get; set; }
        public string Purpose6 { get; set; }
        public string AddPurpose { get; set; }

        public string SearchFor1 { get; set; }
        public string SearchFor2 { get; set; }
        public string AddSearchFor { get; set; }
        public string Job1 { get; set; }
        public string Job2 { get; set; }
        public string Job3 { get; set; }
        public string AddJob { get; set; }


        public string Music1 { get; set; }
        public string Music2 { get; set; }
        public string Music3 { get; set; }
        public string Music4 { get; set; }
        public string Music5 { get; set; }
        public string Music6 { get; set; }
        public string Music7 { get; set; }
        public string Music8 { get; set; }
        public string Music9 { get; set; }
        public string Music10 { get; set; }

        public string AddLifePriority { get; set; }

        public bool IsPost { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Year { get; set; }

        public List<string> Jobs { get; set; }
        public List<string> Purposes { get; set; }
        public List<string> SearchFors { get; set; }
        public List<string> Musics { get; set; }

        public string Avatar
        {
            get
            {
                if (UserPhotos.Any(x=> x.IsAvatar))
                {
                    return UserPhotos.First(x=> x.IsAvatar).Path;
                }
                if (UserPhotos.Any())
                {
                    return UserPhotos.First().Path;
                }
                if (Sex ?? true)
                    return "/pic/user-2.jpg";
                return "/pic/user-1.jpg";
            }
        }

        public string NameAndSurname
        {
            get { return UserName + " " + UserSurname; }
        }

        public decimal Balance
        {
            get { return MoneyTransactions.Where(x => x.Status == 1).Sum(x => x.Sum); }
        }

        public List<string> DefLifePrioritys = new List<string>()
        {
            "Семья и дети"
        };


        public List<string> DefPurposes = new List<string>()
        {
            "Для брака, семьи",
            "Для дружбы",
            "Для любви, романтических отношений",
            "Для секса",
            "Для совместного занятия хобби",
            "Для совместного проживания"
        };

        public List<string> DefSearchFor = new List<string>()
        {
            "Менеджер",
            "Руководитель",
            "Водитель"
        };
        public List<string> DefJobs = new List<string>()
        {
            "Девушку",
            "Парня"
        };

        public void InitAddFieldsFromMain()
        {
            if (BirthDate.HasValue)
            {
                Month = BirthDate.Value.Month;
                Day = BirthDate.Value.Day;
                Year = BirthDate.Value.Year;
            }

            Purposes = (Purpose ?? "").Split<string>(";").ToList();
            AddPurpose = Purposes.Where(x => DefPurposes.All(z => z != x)).JoinToString(";");

            SearchFors = (SearchFor ?? "").Split<string>(";").ToList();
            AddSearchFor = SearchFors.Where(x => DefSearchFor.All(z => z != x)).JoinToString(";");

            Jobs = (Job ?? "").Split<string>(";").ToList();
            AddJob = Jobs.Where(x => DefJobs.All(z => z != x)).JoinToString(";");

            if (!DefLifePrioritys.Contains(LifePriority))
            {
                AddLifePriority = LifePriority;
                LifePriority = "";
            }

            Musics = (Music ?? "").Split<string>(";").ToList();
        }

        public void SaveFiledsToMain()
        {
            BirthDate = new DateTime(Year.Value, Month.Value, Day.Value);
            var p = new[] { Purpose1, Purpose2, Purpose3, Purpose4, Purpose5, Purpose6, AddPurpose };
            Purpose = p.Where(x => x.IsFilled()).JoinToString(";");

            var s = new[] { SearchFor1, SearchFor2, AddSearchFor };
            SearchFor = s.Where(x => x.IsFilled()).JoinToString(";");

            var j = new[] { Job1, Job2, Job3, AddJob };
            Job = j.Where(x => x.IsFilled()).JoinToString(";");

            if (AddLifePriority.IsFilled())
            {
                LifePriority = AddLifePriority;
            }
            var m = new[] { Music1, Music2, Music3, Music4, Music5, Music6, Music7, Music8, Music9, Music10 };
            Music = m.Where(x => x.IsFilled()).JoinToString(";");
        }

        public bool IsLocked(User user)
        {
            return new xDBDataContext().UserLocks.Any(x => (x.TargetID == user.ID && x.UserID == ID) || (x.TargetID == ID && x.UserID == user.ID));
        }
        public bool IsUserLockedMe(User user)
        {
            return new xDBDataContext().UserLocks.Any(x => (x.TargetID == ID && x.UserID == user.ID));
        }
        public bool IsILockedUser(User user)
        {
            return new xDBDataContext().UserLocks.Any(x => (x.TargetID == user.ID && x.UserID == ID));
        }

        public bool IsFriends(User currentUser)
        {
            return
                new xDBDataContext().Friends.Any(
                    x =>
                        (x.FromUserID == currentUser.ID && x.ToUserID == ID) ||
                        (x.FromUserID == ID && x.ToUserID == currentUser.ID));
        }

        public bool IsDated(User currentUser)
        {
            return
            new xDBDataContext().Datings.Any(
                x =>
                    (x.FromUserID == currentUser.ID && x.ToUserID == ID) ||
                    (x.FromUserID == ID && x.ToUserID == currentUser.ID));
        }
    }

    public class MyDatings
    {
        public List<Dating> Income { get; set; }
        public List<Dating> Outcome { get; set; }
        public List<Dating> OnDate { get; set; }

        public MyDatings(xDBDataContext db, User user)
        {
            Income =
                db.Datings.Where(x => x.ToUserID == user.ID && x.Status==0)
                    .OrderByDescending(x => x.IsRead)
                    .ThenByDescending(x => x.CreateDate).ToList();
            Outcome =
                db.Datings.Where(x => x.FromUserID == user.ID)
                    .OrderByDescending(x => x.IsRead)
                    .ThenByDescending(x => x.CreateDate)
                    .ToList();
            OnDate =
                db.Datings.Where(x => (x.FromUserID == user.ID || x.ToUserID == user.ID) && x.Status == 1)
                    .OrderByDescending(x => x.CreateDate)
                    .ToList();
        }
    }
    
    public partial class Dating
    {
        public string GetFriendName(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.NameAndSurname;
            if (FromUserID == currentUser.ID)
                return ToUser.NameAndSurname;
            return "";
        }
        public string GetAge(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Age;
            if (FromUserID == currentUser.ID)
                return ToUser.Age;
            return "";
        }
        public string GetTown(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Town.Name;
            if (FromUserID == currentUser.ID)
                return ToUser.Town.Name;
            return "";
        }

        public string GetAvatar(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Avatar;
            if (FromUserID == currentUser.ID)
                return ToUser.Avatar;
            return "";
        }

        public int GetUserID(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUserID;
            if (FromUserID == currentUser.ID)
                return ToUserID;
            return 0;
        }
    }

    public partial class Friend
    {
        public string GetFriendName(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.NameAndSurname;
            if (FromUserID == currentUser.ID)
                return ToUser.NameAndSurname;
            return "";
        }
        public string GetAge(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Age;
            if (FromUserID == currentUser.ID)
                return ToUser.Age;
            return "";
        }
        public string GetTown(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Town.Name;
            if (FromUserID == currentUser.ID)
                return ToUser.Town.Name;
            return "";
        }

        public string GetAvatar(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUser.Avatar;
            if (FromUserID == currentUser.ID)
                return ToUser.Avatar;
            return "";
        }

        public int GetUserID(User currentUser)
        {
            if (ToUserID == currentUser.ID)
                return FromUserID;
            if (FromUserID == currentUser.ID)
                return ToUserID;
            return 0;
        }

        public bool IsAlreadyInvitedForEvent(User currentUser, Event meet)
        {
            var db = new xDBDataContext();
            return db.EventPeoples.Any(
                x =>
                    ((x.FromUserID == currentUser.ID && x.ToUserID == GetUserID(currentUser)) ||
                     (x.ToUserID == currentUser.ID && x.FromUserID == GetUserID(currentUser))) && x.EventID == meet.ID);
        }
    }



    public class Friendship
    {
        public List<Friend> Friends { get; set; }
        public int FriendCount { get; set; }
        public List<Friend> FriendInvitesIncome { get; set; }
        public int FriendInvitesIncomeCount { get; set; }
        public int FriendInvitesIncomeCountNew { get; set; }
        public List<Friend> FriendInvitesOutcome { get; set; }

        public int Order { get; set; }
        public string Search { get; set; }

        public Friendship(xDBDataContext db, string search, int order, User user)
        {
            Order = order;
            Search = search;
            var list = db.Friends.Where(x => x.IsRead && (x.FromUserID == user.ID || x.ToUserID == user.ID) && x.Status == 1);
            if (search.IsFilled())
            {
                list = FIlterByWord(list, search, user);
            }
            if (order == 0)
            {
                Friends = list.OrderByDescending(x => x.CreateDate).ToList();
            }
            else
            {
                Friends =
                    list.ToList()
                        .Select(
                            x =>
                                new
                                {
                                    Value = x,
                                    Name = (x.ToUserID == user.ID ? x.FromUser.NameAndSurname : x.ToUser.NameAndSurname)
                                })
                        .OrderBy(x => x.Name)
                        .Select(x => x.Value)
                        .ToList();
            }
            FriendCount = Friends.Count;

            var income = db.Friends.Where(x => x.Status == 0 && x.ToUserID == user.ID);
            if (search.IsFilled())
            {
                income = FIlterByWord(income, search, user);
            }
            if (order == 0)
            {
                FriendInvitesIncome = income.OrderByDescending(x => x.CreateDate).ToList();
            }
            else
            {
                FriendInvitesIncome =
                    income.ToList().OrderBy(x => x.FromUser.NameAndSurname).ToList();
            }
            FriendInvitesIncomeCount = FriendInvitesIncome.Count;
            FriendInvitesIncomeCountNew = FriendInvitesIncome.Count(x => !x.IsRead);

            var outcome = db.Friends.Where(x => x.FromUserID == user.ID);
            if (search.IsFilled())
            {
                outcome = FIlterByWord(outcome, search, user);
            }
            if (order == 0)
            {
                FriendInvitesOutcome = outcome.OrderByDescending(x => x.CreateDate).ToList();
            }
            else
            {
                FriendInvitesOutcome =
                    outcome.ToList().OrderBy(x => x.ToUser.NameAndSurname).ToList();
            }
        }

        private IQueryable<Friend> FIlterByWord(IQueryable<Friend> list, string search, User user)
        {
            return
                list.Where(
                    x =>
                        (x.ToUserID == user.ID &&
                         (SqlMethods.Like(x.FromUser.UserName,
                             "{0}%".FormatWith(search)) ||
                          SqlMethods.Like(x.FromUser.UserSurname, "{0}%".FormatWith(search)))) ||
                        (x.FromUserID == user.ID &&
                         (SqlMethods.Like(x.ToUser.UserName, "{0}%".FormatWith(search)) ||
                          SqlMethods.Like(x.ToUser.UserPatrinomic, "{0}%".FormatWith(search)))));
        }
    }



    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public string Message { get; set; }
        public string RedirectURL { get; set; }
    }
    public class RegisterModel
    {
        public string Photo { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int? Town { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public bool Sex { get; set; }

        public bool IsPost { get; set; }
    }
    [Serializable]
    public class UserDataFromNetwork
    {
        public string network { get; set; }
        public string identity { get; set; }
        public string uid { get; set; }
        public string email { get; set; }
        public string nickname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string bdate { get; set; }
        public string photo { get; set; }
        public string photo_big { get; set; }
        public string city { get; set; }
        public string profile { get; set; }
        public int verified_email { get; set; }
        public int sex { get; set; }

    }

    public class SocialAuthResult
    {
        public User User { get; set; }
        public bool IsNew { get; set; }
        public static SimpleMembershipProvider MembershipProvider
        {
            get
            {
                return (SimpleMembershipProvider)Membership.Provider;
            }
        }

        public static SimpleRoleProvider RoleProvider
        {
            get
            {
                return (SimpleRoleProvider)Roles.Provider;
            }
        }

        public string Message { get; set; }
        public bool HasResult { get; set; }

        private static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }


        public static string GeneratePassword(int len = 6)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            string pattern = "0123456789abcdefghijklmnopqrstuvwxyz";
            string pass = "";
            if (len == 0) len = 6;
            for (int i = 0; i < len; i++)
            {
                pass += pattern.Substring(rnd.Next(0, pattern.Length), 1);
            }
            return pass;
        }


        public static SocialAuthResult CheckAuth()
        {
            /*
                        var from = HttpContext.Current.Request["from"];
                        if (from.IsNullOrWhiteSpace())
                            return new SocialAuthResult();
            */
            bool isNew = false;
            var target = String.Format("http://ulogin.ru/token.php?token={0}&host={1}", HttpContext.Current.Request["token"],
                                       HttpContext.Current.Request.Url.Host);

            var wc = new WebClient();
            byte[] data = null;
            try
            {
                data = wc.DownloadData(target);
            }
            catch (Exception exxxx)
            {
                return new SocialAuthResult()
                {
                    HasResult = true,
                    Message = "Ошибка при установлении соединения с сервером авторизации",
                };
            }
            var js = Encoding.UTF8.GetString(data);
            js = DecodeEncodedNonAsciiCharacters(js);
            var serializer = new JavaScriptSerializer();
            var jsData = serializer.Deserialize<UserDataFromNetwork>(js);

            if (string.IsNullOrEmpty(jsData.email))
            {
                return new SocialAuthResult()
                {
                    HasResult = true,
                    Message = "Для регистрации через соцсеть, в соцсети должен быть указан email",
                };
            }

            User user = null;
            try
            {

                var db =
                    new xDBDataContext(
                        ConfigurationManager.ConnectionStrings["SexiLoveConnectionString"].ConnectionString);
                user = db.Users.FirstOrDefault(x => x.Email.ToLower() == jsData.email.ToLower());


                //нет такого
                if (user == null)
                {
                    var pass = GeneratePassword(6);
                    DateTime bd = DateTime.MinValue;
                    DateTime.TryParse(jsData.bdate, out bd);

                    var dict = new Dictionary<string, object>();
                    if (bd != DateTime.MinValue)
                    {
                        dict.Add("BirthDate", bd);
                    }
                    var town = db.Towns.FirstOrDefault(x => x.Name.ToLower() == (jsData.city ?? "").ToLower());
                    if (town == null)
                    {
                        town = db.Towns.FirstOrDefault(x => x.Name.ToLower() == "москва");
                    }
                    if (town == null)
                    {
                        town = db.Towns.First();
                    }
                    dict.Add("TownID", town.ID);
                    dict.Add("Email", jsData.email);
                    dict.Add("Sex", jsData.sex == 2);


                    dict.Add("UserName", (jsData.first_name ?? ""));
                    dict.Add("UserSurname", (jsData.last_name ?? ""));
                    MembershipProvider.CreateUserAndAccount(jsData.email, pass, false, dict);
                    RoleProvider.AddUsersToRoles(new[] { jsData.email }, new[] { "Client" });
                    var userDB = db.Users.FirstOrDefault(x => x.Email == jsData.email);
                    var al = jsData.photo_big.IsNullOrEmpty() ? jsData.photo : jsData.photo_big;
                    if (!al.IsNullOrEmpty())
                    {
                        var ext = al.Substring(al.Length - 4);
                        string ap = "/Content/Avatars/" + Guid.NewGuid() +
                                    (ext.StartsWith(".")
                                        ? ext
                                        : ".jpg");

                        try
                        {

                            wc.DownloadFile(al, HttpContext.Current.Server.MapPath(ap));

                            if (userDB != null)
                            {
                                db.UserPhotos.InsertOnSubmit(new UserPhoto() { Path = ap, UserID = userDB.ID });
                                db.SubmitChanges();
                            }
                        }
                        catch
                        {

                        }
                    }

                    WebSecurity.Login(jsData.email, pass, true);
                    user = userDB;
                    isNew = true;
                }
                //есть чувак
                else
                {
                    //мыло подтверждено и совпало, логин совпал
                    if (( /*jsData.verified_email == 1 && */jsData.email.ToLower() == user.Email.ToLower()))
                    {
                        FormsAuthentication.SetAuthCookie(jsData.email, false);
                    }
                    //редирект на страницу с формой, где выводим сообщение
                    else
                    {
                        return new SocialAuthResult()
                        {
                            HasResult = false,
                            Message = (jsData.nickname == user.Email
                                ? "Пользователь с таким логином уже зарегистрирован. Пожалуйста, укажите другой логин."
                                : "Пользователь с таким Email уже зарегистрирован. Пожалуйста укажите другой Email"),
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                return new SocialAuthResult()
                {
                    HasResult = false,
                    Message = ex.Message,
                    IsNew = isNew
                };

            }

            return new SocialAuthResult()
            {
                User = user,
                HasResult = true,
                Message = "",
                IsNew = isNew
            };

        }
    }

}