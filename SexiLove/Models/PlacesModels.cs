using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{

    public partial class Place
    {
        public bool IsPost { get; set; }
        public string Image { get; set; }

        public bool WishToBeAdmin { get; set; }

        public int CommonRating
        {
            get { return PlaceRatings.Where(x => x.Name == "").Sum(x => x.Rating); }
        }

        public List<string> AlowedAtmos(User currentUser)
        {
            var exist = PlaceRatings.Where(x => x.UserID == currentUser.ID && x.Name != "").Select(x=> x.Name);
            return PlacesSearch.AtmoList.Except(exist).ToList();
        }
    }

    public class PlacesSearch
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public int? TownID { get; set; }
        public int? MinPay { get; set; }
        public int? MaxPay { get; set; }

        public string Atmos { get; set; }

        public List<Place> Result { get; set; }

        public static List<string> AtmoList
        {
            get
            {
                return new List<string>()
                {
                    "Демократично",
                    "Гламурно",
                    "Экономично",
                    "По-хипстерски",
                    "Дорого",
                    "Пафосно",
                    "Необычно",
                    "Весело",
                    "Шумно",
                    "Тихо",
                    "Уютно",
                    "Красиво",
                    "Андеграунд",
                    "Хорошая кухня",
                    "Хорошая музыка",
                    "Хорошая публика"
                };


            }
        }


        public PlacesSearch()
        {
            Type = "Кафе";
            TownID = 1;
            Order = 1;
        }
        public void Search(xDBDataContext db)
        {
            var places = db.Places.Where(x=> x.Approved).AsQueryable();
            if (TownID.HasValue)
            {
                places = places.Where(x => x.TownID == TownID);
            }
            if (Type.IsFilled())
            {
                places = places.Where(x => x.Type == Type);
            }
            if (Atmos.IsFilled())
            {
                places = places.Where(x => x.PlaceRatings.Any(z => z.Name == Atmos));
            }
            if (MinPay.HasValue)
            {
                places = places.Where(x => x.MinPay >= MinPay);
            }
            if (MaxPay.HasValue)
            {
                places = places.Where(x => x.MaxPay <= MaxPay);
            }
            Result = places.ToList();
            if (Order == 2)
            {
                Result = Result.OrderByDescending(x => x.CommonRating).ToList();
            }
            else
            {
                Result = Result.OrderBy(x => x.Name).ToList();
            }
        }
    }
}