using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using SexiLove.Extensions.Mail;
using WebMatrix.WebData;

namespace SexiLove.Models
{
    public class BaseController : Controller
    {
        public static string HostName = string.Empty;
        protected override void Initialize(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.Url != null)
            {
                HostName = requestContext.HttpContext.Request.Url.Authority;
            }
            base.Initialize(requestContext);
        }


        private IConfig _config;
        public IConfig Config
        {
            get { return _config ?? (_config = new Config()); }
        }

        private xDBDataContext _db;
        public xDBDataContext DB
        {
            get { return _db ?? (_db = new xDBDataContext(ConfigurationManager.ConnectionStrings["SexiLoveConnectionString"].ConnectionString)); }
        }

        public SimpleMembershipProvider MembershipProvider
        {
            get
            {
                return (SimpleMembershipProvider)Membership.Provider;
            }
        }

        public SimpleRoleProvider RoleProvider
        {
            get
            {
                return (SimpleRoleProvider)Roles.Provider;
            }
        }



        private User _currentUser;
        public User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    _currentUser = DB.Users.FirstOrDefault(x => x.ID == WebSecurity.CurrentUserId);
                    if (_currentUser != null)
                        _currentUser.InitAddFieldsFromMain();
                }
                return _currentUser;
            }
            set { _currentUser = value; }
        }
    }
}