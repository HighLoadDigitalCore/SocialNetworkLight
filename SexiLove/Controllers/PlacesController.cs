using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SexiLove.Extensions;
using SexiLove.Models;

namespace SexiLove.Controllers
{
    public class PlacesController : BaseController
    {

        public ActionResult AllPhoto(int ID)
        {
            var list = DB.PlaceImages.Where(x => x.PlaceID == ID).ToList();
            return View(list);
        }


        [Authorize]
        [HttpPost]
        public ActionResult RateAtmos(int PlaceID, string Atmo)
        {
            var place = DB.Places.FirstOrDefault(x => x.ID == PlaceID);
            if(place==null || Atmo.IsNullOrEmpty())
                return new ContentResult();

            var my = DB.PlaceRatings.Where(x => x.UserID == CurrentUser.ID && x.PlaceID == PlaceID && x.Name != "");
            if (my.Count() == 3)
            {
                return new ContentResult();
            }
            var mark = new PlaceRating()
            {
                Name = Atmo,
                PlaceID = PlaceID,
                Rating = 10,
                UserID = CurrentUser.ID,

            };
            DB.PlaceRatings.InsertOnSubmit(mark);
            DB.SubmitChanges();
            return new ContentResult();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Places()
        {
            return View();
        }

        [HttpGet]
        public ActionResult View(int ID)
        {
            return View(DB.Places.FirstOrDefault(x => x.ID == ID));
        }

        [HttpGet]
        public ActionResult Search()
        {
            var dating = new PlacesSearch();
            if (CurrentUser != null)
            {
                dating.TownID = CurrentUser.TownID;
            }
            dating.Search(DB);
            return PartialView(dating);
        }
        [HttpPost]
        public ActionResult Search(PlacesSearch dating)
        {
            dating.Search(DB);
            return PartialView(dating);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            return View(new Place());
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Place place, HttpPostedFileBase Logo, HttpPostedFileBase Image)
        {
            place.IsPost = true;
            if (Logo == null || Logo.ContentLength == 0 || Image == null || Image.ContentLength == 0 ||
                place.Name.IsNullOrEmpty() || place.Adress.IsNullOrEmpty() || place.Description.IsNullOrEmpty() ||
                !place.MinPay.HasValue || !place.MaxPay.HasValue ||
                place.Type.IsNullOrEmpty() || (place.WishToBeAdmin && !place.AdminMail.IsMailAdress() ) )
                return View(place);


            string fileNameLogo = Guid.NewGuid() + Path.GetExtension(Logo.FileName);
            string pathLogo = "/Content/Places/" + fileNameLogo;
            Logo.SaveAs(Server.MapPath(pathLogo));

            string fileNameImage = Guid.NewGuid() + Path.GetExtension(Image.FileName);
            string pathImage = "/Content/Places/" + fileNameImage;
            Image.SaveAs(Server.MapPath(pathImage));

            place.Logo = pathLogo;
            DB.PlaceImages.InsertOnSubmit(new PlaceImage() { Path = pathImage, Place = place });
            DB.Places.InsertOnSubmit(place);

            DB.SubmitChanges();

            return RedirectToAction("AfterCreate", "Places", new{ID = place.ID});
        }

        [Authorize]
        public ActionResult AfterCreate(int ID)
        {
            var place = DB.Places.FirstOrDefault(x => x.ID == ID);
            return PartialView(place);
        }

        [Authorize]
        public ActionResult Like(int id)
        {
            var place = DB.Places.FirstOrDefault(x => x.ID == id);
            if (place != null)
            {
                var mark = DB.PlaceRatings.Any(x => x.UserID == CurrentUser.ID && x.Name == "" && x.PlaceID == id);
                if (!mark)
                {
                    DB.PlaceRatings.InsertOnSubmit(new PlaceRating()
                    {
                        UserID = CurrentUser.ID,
                        Name = "",
                        PlaceID = id,
                        Rating = 10
                    });
                    DB.SubmitChanges();
                }

            }
            return RedirectToAction("View", new {ID = id});
        }
    }
}
