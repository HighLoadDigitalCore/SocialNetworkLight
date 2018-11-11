using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{

    public partial class EventPeople
    {
        public bool IsNotMe(User currentUser)
        {
            if (FromUserID == currentUser.ID && !ToUserID.HasValue)
                return false;
            return true;

        }

        public string GetName(User currentUser)
        {
            if (ToUserID == null)
            {
                return FromUser.NameAndSurname;
            }
            if (ToUserID == currentUser.ID)
                return FromUser.NameAndSurname;
            if (FromUserID == currentUser.ID)
                return ToUser.NameAndSurname;
            return "";
        }
        public string GetAge(User currentUser)
        {
            if (ToUserID == null)
            {
                return FromUser.Age;
            }
            if (ToUserID == currentUser.ID)
                return FromUser.Age;
            if (FromUserID == currentUser.ID)
                return ToUser.Age;
            return "";
        }
        public string GetTown(User currentUser)
        {
            if (ToUserID == null)
            {
                return FromUser.Town.Name;
            }
            if (ToUserID == currentUser.ID)
                return FromUser.Town.Name;
            if (FromUserID == currentUser.ID)
                return ToUser.Town.Name;
            return "";
        }

        public string GetAvatar(User currentUser)
        {
            if (ToUserID == null)
            {
                return FromUser.Avatar;
            }
            if (ToUserID == currentUser.ID)
                return FromUser.Avatar;
            if (FromUserID == currentUser.ID)
                return ToUser.Avatar;
            return "";
        }

        public int GetUserID(User currentUser)
        {
            if (ToUserID == null)
            {
                return FromUserID;
            }
            if (ToUserID == currentUser.ID)
                return FromUserID;
            if (FromUserID == currentUser.ID)
                return ToUserID ?? 0;
            return 0;
        }

        public bool IsAlreadyInvitedForEvent(User currentUser)
        {
            var db = new xDBDataContext();
            return db.EventPeoples.Any(
                x =>
                    ((x.FromUserID == currentUser.ID && x.ToUserID == GetUserID(currentUser)) ||
                     (x.ToUserID == currentUser.ID && x.FromUserID == GetUserID(currentUser))) && x.EventID == ID);

        }
    }

    public class EventsSearch
    {
        public string CatUrl { get; set; }
        public int? TownID { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public string Name { get; set; }
        public User Me { get; set; }

        public List<Event> Result { get; set; }

        public List<EventCat> Cats { get; set; }

        public EventsSearch()
        {

        }

        public EventsSearch(User me)
        {
            if (me != null)
            {
                if (!TownID.HasValue)
                    TownID = me.TownID;
                Me = me;

            }
            else
            {
                if (!TownID.HasValue)
                    TownID = 1;
            }
        }

        public void Search(xDBDataContext db)
        {
            var res = db.Events.AsQueryable();
            if (MinDate.HasValue)
            {
                res = res.Where(x => x.StartDate.HasValue && x.StartDate.Value.Date >= MinDate.Value.Date);
            }
            if (MaxDate.HasValue)
            {
                res = res.Where(x => x.StartDate.HasValue && x.StartDate.Value.Date <= MaxDate.Value.Date);
            }
            if (!MinDate.HasValue && !MaxDate.HasValue)
            {
                res = res.Where(x => x.StartDate.HasValue && x.StartDate.Value.Date >= DateTime.Now.Date);
            }
            if (TownID.HasValue)
            {
                res = res.Where(x => x.EventCatsForTown.TownID == TownID);
            }

            if (Name.IsFilled())
            {
                res =
                    res.Where(
                        x => SqlMethods.Like(x.Name.ToLower(), "%" + Name.Trim().ToLower().Replace(" ", "%") + "%"));
            }

            if (CatUrl.IsFilled())
            {
                res = res.Where(x => x.EventCatsForTown.EventCat.ExportURL == CatUrl);
            }

            Result = res.OrderBy(x => x.StartDate).ToList();

            Cats =
                db.Events.Where(x => x.StartDate.HasValue && x.EventCatsForTown.TownID == TownID)
                    .Select(x => x.EventCatsForTown.EventCat)
                    .Distinct()
                    .ToList();
        }

        public void LoadCats()
        {

            Cats =
                new xDBDataContext().Events.Where(x => x.StartDate.HasValue && x.EventCatsForTown.TownID == TownID)
                    .Select(x => x.EventCatsForTown.EventCat)
                    .Distinct()
                    .ToList();
        }
    }


    public class MyEvents
    {

        public int ImGoListCount { get; set; }
        public List<EventPeople> ImGoList { get; set; }
        public int IneedCompanyCount { get; set; }
        public List<EventPeople> IneedCompanyList { get; set; }
        public int InviteCount { get; set; }
        public List<EventPeople> InviteList { get; set; }

        public int Order { get; set; }
        public string Search { get; set; }

        public MyEvents(xDBDataContext db, string search, int order, User user)
        {
            Order = order;
            Search = search;
            var list = db.EventPeoples.Where(x => (x.FromUserID == user.ID || x.ToUserID == user.ID) && x.Status == 1 && x.Type == 1);
            if (search.IsFilled())
            {
                list = FIlterByWord(list, search, user);
            }
            if (order == 0 || order==3)
            {
                ImGoList = list.OrderByDescending(x => x.SendDate).Take(10).ToList();
            }
            else if (order == 1)
            {
                ImGoList = list.OrderBy(x => x.SendDate).ToList();

            }
            else if (order == 2)
            {
                ImGoList = list.OrderByDescending(x => x.SendDate).ToList();

            }
            ImGoList = ImGoList.GroupBy(x => x.EventID).Select(x => x.First()).ToList();
            ImGoListCount = ImGoList.Count;

            var need = db.EventPeoples.Where(x => x.Status == 1 && x.FromUserID == user.ID && x.Type == 0);
            if (search.IsFilled())
            {
                need = FIlterByWord(need, search, user);
            }
            if (order == 0 || order == 3)
            {
                IneedCompanyList = need.OrderByDescending(x => x.SendDate).Take(10).ToList();
            }
            else if (order == 1)
            {
                IneedCompanyList = need.OrderBy(x => x.SendDate).ToList();
            }
            else if (order == 2)
            {
                IneedCompanyList = need.OrderByDescending(x => x.SendDate).ToList();
            }
            IneedCompanyCount = IneedCompanyList.Count;

            var invites = db.EventPeoples.Where(x => x.ToUserID == user.ID);
            if (search.IsFilled())
            {
                invites = FIlterByWord(invites, search, user);
            }
            if (order == 0)
            {

            }
            else if (order == 1)
            {
                invites = invites.Where(x => x.Status == 1);
            }
            else if (order == 2)
            {
                invites = invites.Where(x => x.Status == -1);
            }
            else if (order == 3)
            {
                invites = invites.Where(x => x.Status == 0);
            }
            InviteList = invites.ToList();
            InviteCount = InviteList.Count;
        }

        private IQueryable<EventPeople> FIlterByWord(IQueryable<EventPeople> list, string search, User user)
        {
            return list.Where(x => SqlMethods.Like(x.Event.Name, "%{0}%".FormatWith(search)));
            /*
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
            */
        }
    }

}