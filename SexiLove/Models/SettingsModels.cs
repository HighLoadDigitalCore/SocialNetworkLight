using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{
    public partial class UserSetting
    {
        public string OldPass { get; set; }
        public string NewPass { get; set; }


        public string OldMail { get; set; }
        public string NewMail { get; set; }

        public bool IsPost { get; set; }
        public string Error { get; set; }

        public static bool HasAccess(User source, User target, string Access)
        {
            var settings = source.UserSetting ?? new UserSetting()
            {
                ViewFriends = 0,
                ViewMeetings = 0,
                ViewPhotos = 0,
                OfferFriendship = 0,
                SendLetters = 0,
                InviteForMeeting = 0
            };

            int prop = (int) settings.GetPropertyValue(Access);
            if (prop == 0)
                return true;
            if (prop == -1)
                return false;
            if (prop == 1)
            {
                return
                    new xDBDataContext().Friends.Any(
                        x => x.Status == 1 &&
                             ((x.FromUserID == source.ID && x.ToUserID == target.ID) ||
                              (x.FromUserID == target.ID && x.ToUserID == source.ID)));
            }
            
            if (prop == 2)
            {
                var db = new xDBDataContext();
                var fr = db.Friends.Any(
                    x => x.Status == 1 &&
                         ((x.FromUserID == source.ID && x.ToUserID == target.ID) ||
                          (x.FromUserID == target.ID && x.ToUserID == source.ID)));
                if (fr)
                    return true;

                //Все мои друзья
                var friends = db.Friends.Where(
                    x => x.Status == 1 &&
                         ((x.FromUserID == source.ID) ||
                          (x.ToUserID == source.ID))).ToList();


                var sec = new List<Friend>();

                foreach (var friend in friends)
                {
                    var sf = db.Friends.Where(
                        x => x.Status == 1 &&
                             ((x.FromUserID == friend.ID) ||
                              (x.ToUserID == friend.ID))).ToList();
                    sec.AddRange(sf);
                }


                return sec.Any(x => x.FromUserID == target.ID || x.ToUserID == target.ID);
            }

            return false;
        }
    }
}