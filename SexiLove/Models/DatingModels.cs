using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{
    public class DatingSearch
    {
        public int? TownID { get; set; }
        public bool Sex { get; set; }
        public string Purpose { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool HasPhoto { get; set; }

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

        public bool IsOnline { get; set; }

        public List<User> Users { get; set; }

        public DatingSearch()
        {
            /*
                        Sex = false;
                        Purpose = "Любви, романтических отношений";
                        MinAge = 18;
                        MaxAge = 45;
                        HasPhoto = true;
            */
        }

        public DatingSearch(User me)
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
            HasPhoto = true;
        }
        public void Search(xDBDataContext db)
        {
            var users = db.Users.AsQueryable();
            users = users.Where(x => x.Sex == Sex);
            if (Purpose.IsFilled())
            {
                users = users.Where(x => SqlMethods.Like(x.Purpose, string.Format("%{0}%", Purpose)));
            }
            if (MinAge.HasValue)
                users = users.Where(x => x.BirthDate.HasValue && x.BirthDate.Value.Date <= MinBirthDate.Value.Date);
            if (MaxAge.HasValue)
                users = users.Where(x => x.BirthDate.HasValue && x.BirthDate.Value.Date >= MaxBirthDate.Value.Date);
            if (HasPhoto)
                users = users.Where(x => x.UserPhotos.Any());
            if (IsOnline)
            {
                users =
                    users.Where(x => x.LastVisitDate.HasValue && x.LastVisitDate.Value >= DateTime.Now.AddMinutes(-5));
            }
            if (TownID.HasValue)
            {
                users = users.Where(x => x.TownID == TownID.Value);
            }

            Users = users.ToList().OrderByDescending(x=> x.IsVip).ThenByDescending(x=> x.IsUp).ThenByDescending(x=> x.webpages_Membership.CreateDate).ToList();
        }
    }
}