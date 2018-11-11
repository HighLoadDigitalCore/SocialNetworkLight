using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{

    public partial class Meeting
    {
        public string FriendList { get; set; }
        public bool IsPost { get; set; }
        public string TimeStr { get; set; }

        public string Years
        {
            get
            {
                if (MinAge.HasValue && !MaxAge.HasValue)
                {
                    return MinAge.Value.ToString() + " лет";
                }
                if (!MinAge.HasValue && MaxAge.HasValue)
                {
                    return MaxAge.Value.ToString() + " лет";
                }
                if (MinAge.HasValue && MaxAge.HasValue)
                {
                    return MinAge.Value.ToString() + "-" + MaxAge.Value.ToString() + " лет";
                }
                return "";
            }
        }
    }

    public class MeetingShortSearch
    {
        public int? TownID { get; set; }
        public bool Sex { get; set; }
        public string Purpose { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public string Keywords { get; set; }
        public bool Compartible { get; set; }

        public User Me { get; set; }


        public List<Meeting> Result { get; set; }

        public DateTime? MinBirthDate
        {
            get
            {
                if (!MinAge.HasValue)
                    return null;

                return new DateTime(DateTime.Now.Year - MinAge ?? 0, DateTime.Now.Month, DateTime.Now.Day);
            }
        }
        public DateTime? MaxBirthDate
        {
            get
            {
                if (!MaxAge.HasValue)
                    return null;

                return new DateTime(DateTime.Now.Year - MaxAge ?? 0, DateTime.Now.Month, DateTime.Now.Day);
            }
        }

        public MeetingShortSearch()
        {

        }
        public MeetingShortSearch(User me)
        {
            if (me != null)
            {
                Sex = !(me.Sex ?? true);
                var pps = (me.Purpose ?? "").Split<string>(";").ToList();
                if (pps.Any())
                    Purpose = pps.First();
                if (me.MyAge.HasValue)
                {
                    MinAge = me.MyAge - 5;
                    MaxAge = me.MyAge + 5;
                }
                if (!TownID.HasValue)
                    TownID = me.TownID;
                Me = me;

            }
            else
            {
                Sex = false;
                Purpose = "";
                MinAge = 18;
                MaxAge = 45;
                if (!TownID.HasValue)
                    TownID = 1;
            }
        }

        public void Search(xDBDataContext db)
        {
            var res = db.Meetings.AsQueryable();
            res = res.Where(x => x.TargetSex == !Sex);
            if (Purpose.IsFilled())
            {
                res = res.Where(x => x.Type == Purpose);
            }
            if (MinAge.HasValue)
            {
                res = res.Where(x => x.User.BirthDate.HasValue && x.User.BirthDate <= MinBirthDate);
            }
            if (MaxAge.HasValue)
            {
                res = res.Where(x => x.User.BirthDate.HasValue && x.User.BirthDate >= MaxBirthDate);
            }
            if (Keywords.IsFilled())
            {
                res = res.Where(x => SqlMethods.Like(x.Comment, "%{0}%".FormatWith(Keywords.Replace(" ", "%"))));
            }
            if (MinDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date >= MinDate.Value.Date);
            }
            if (MaxDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date <= MaxDate.Value.Date);
            }
            if (!MinDate.HasValue && !MaxDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date >= DateTime.Now.Date);
            }
            if (TownID.HasValue)
            {
                res = res.Where(x => x.User.TownID == TownID);
            }
            if (Compartible && Me != null)
            {
                res =
                    res.Where(
                        x =>
                            (x.MaxAge.HasValue && x.MaxAge <= Me.MyAge && x.MinAge.HasValue &&
                             x.MinAge.Value == Me.MyAge) || (!x.MaxAge.HasValue && !x.MinAge.HasValue));
            }

            Result = res.ToList();
        }
    }

    public class MeetingFullSearch
    {
        public List<Place> Places { get; set; }

        public int? TownID { get; set; }//
        public bool Sex { get; set; }//
        public int? PlaceID { get; set; }
        public int? MinAge { get; set; }//
        public int? MaxAge { get; set; }//

        public DateTime? MinDate { get; set; }//
        public DateTime? MaxDate { get; set; }//

        public TimeSpan? MinTime
        {
            get
            {
                try
                {

                    TimeSpan span = TimeSpan.ParseExact(MinTimeStr, "g", CultureInfo.CurrentCulture);
                    return span;
                }
                catch
                {
                    MinTimeStr = "";
                    return null;
                }

            }
            set
            {
                if (value == null)
                {
                    MinTimeStr = "";
                }
                else
                {
                    MinTimeStr = value.Value.Hours + ":" + value.Value.Minutes;
                }
            }
        }
        public TimeSpan? MaxTime
        {
            get
            {
                try
                {

                    TimeSpan span = TimeSpan.ParseExact(MaxTimeStr, "g", CultureInfo.CurrentCulture);
                    return span;
                }
                catch
                {
                    MaxTimeStr = "";
                    return null;
                }

            }
            set
            {
                if (value == null)
                {
                    MaxTimeStr = "";
                }
                else
                {
                    MaxTimeStr = value.Value.Hours + ":" + value.Value.Minutes;
                }
            }
        }

        public string MinTimeStr { get; set; }

        public string MaxTimeStr { get; set; }

        public string Orientation { get; set; }//

        public bool IsForSponsor { get; set; }
        public bool IsFromSponsor { get; set; }
        public bool IsFromFriends { get; set; }
        public bool IsFromSympaty { get; set; }

        public string Keywords { get; set; }//
        public bool Compartible { get; set; }

        public bool HidePayedMeetings { get; set; }


        public List<Meeting> Result { get; set; }

        public User Me { get; set; }

        public DateTime? MinBirthDate
        {
            get
            {
                if (!MinAge.HasValue)
                    return null;

                return new DateTime(DateTime.Now.Year - MinAge ?? 0, DateTime.Now.Month, DateTime.Now.Day);
            }
        }
        public DateTime? MaxBirthDate
        {
            get
            {
                if (!MaxAge.HasValue)
                    return null;

                return new DateTime(DateTime.Now.Year - MaxAge ?? 0, DateTime.Now.Month, DateTime.Now.Day);
            }
        }

        public MeetingFullSearch()
        {
            var db = new xDBDataContext();
            Places = db.Places.Where(x => x.Approved).OrderByDescending(x => x.PlaceRatings.Where(z=> z.Name=="").Sum(z => z.Rating)).ToList();
        }
        public MeetingFullSearch(User me)
        {
            var db = new xDBDataContext();
            Places = db.Places.Where(x => x.Approved).OrderByDescending(x => x.PlaceRatings.Where(z => z.Name == "").Sum(z => z.Rating)).ToList();

            if (me != null)
            {
                Sex = !(me.Sex ?? true);
                
                if (me.MyAge.HasValue)
                {
                    MinAge = me.MyAge - 5;
                    MaxAge = me.MyAge + 5;
                }
                if (!TownID.HasValue)
                    TownID = me.TownID;
                Me = me;

            }
            else
            {
                Sex = false;
                MinAge = 18;
                MaxAge = 45;
                if (!TownID.HasValue)
                    TownID = 1;
            }
        }

        public void Search(xDBDataContext db)
        {
            var res = db.Meetings.AsQueryable();
            res = res.Where(x => x.TargetSex == Sex);
            if (MinAge.HasValue)
            {
                res = res.Where(x => x.User.BirthDate.HasValue && x.User.BirthDate <= MinBirthDate);
            }
            if (MaxAge.HasValue)
            {
                res = res.Where(x => x.User.BirthDate.HasValue && x.User.BirthDate >= MaxBirthDate);
            }
            if (Keywords.IsFilled())
            {
                res = res.Where(x => SqlMethods.Like(x.Comment, "%{0}%".FormatWith(Keywords.Replace(" ", "%"))));
            }
            if (MinDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date >= MinDate.Value.Date);
            }
            if (MaxDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date <= MaxDate.Value.Date);
            }
            if (!MinDate.HasValue && !MaxDate.HasValue)
            {
                res = res.Where(x => x.Date.HasValue && x.Date.Value.Date >= DateTime.Now.Date);
            }
            if (TownID.HasValue)
            {
                res = res.Where(x => x.User.TownID == TownID);
            }
            if (Orientation.IsFilled())
            {
                res = res.Where(x => x.User.Orientation == Orientation);
            }

            var mit = MinTime;
            if (mit.HasValue)
            {

                //res = res.Where(x => x.Date.HasValue && SqlMethods.DateDiffDay(x.Date.Value, x.Date.Value.Date) >= mit.Value.TotalDays);
                res = res.Where(x => x.Time.HasValue && x.Time >= mit);
            }     
            var mat = MaxTime;
            if (mat.HasValue)
            {
                //res = res.Where(x => x.Date.HasValue && SqlMethods.DateDiffDay(x.Date.Value, x.Date.Value.Date) <= mat.Value.TotalDays);
                res = res.Where(x => x.Time.HasValue && x.Time <= mat);
            }


            if (IsFromSponsor && IsForSponsor)
            {
                res = res.Where(x => x.User.Sponsor.Trim() == "Я желаю быть спонсором" || x.User.Sponsor.Trim() == "Я ищу спонсора");
            }
            else if (IsForSponsor)
            {
                res = res.Where(x => x.User.Sponsor.Trim() == "Я ищу спонсора");
            }
            else if (IsFromSponsor)
            {
                res = res.Where(x => x.User.Sponsor.Trim() == "Я желаю быть спонсором");
            }

            if (IsFromFriends && Me!=null)
            {
                res =
                    res.Where(
                        x =>
                            x.User.FriendsFromUser.Any(z => z.Status == 1 && z.ToUserID == Me.ID) ||
                            x.User.FriendsToUser.Any(z => z.Status == 1 && z.FromUserID == Me.ID));
            }

            if (IsFromSympaty)
            {
                res =
                    res.Where(
                        x =>
                            (x.User.DatingsFromUser.Any(z => z.Status == 1 && z.ToUserID == Me.ID) ||
                             x.User.DatingsToUser.Any(z => z.Status == 1 && z.FromUserID == Me.ID)));
            }

            if (Compartible && Me != null)
            {
                res =
                    res.Where(
                        x =>
                            (x.MaxAge.HasValue && x.MaxAge <= Me.MyAge && x.MinAge.HasValue &&
                             x.MinAge.Value == Me.MyAge) || (!x.MaxAge.HasValue && !x.MinAge.HasValue));
            }


            if (HidePayedMeetings)
            {
                res = res.Where(x => !x.Cost.HasValue);
            }

            Result =
                res.ToList()
                    .OrderByDescending(x => x.User.IsVip).ThenByDescending(x=> x.User.IsUp)
                    .ThenByDescending(x => x.Date)
                    .ToList();


        }
    }
}