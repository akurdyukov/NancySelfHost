using System;
using System.Dynamic;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;

namespace NancySelfHost
{
    public class MainModule : NancyModule
    {
        private readonly GuidUserMapper guidUserMapper;

        public MainModule(GuidUserMapper guidUserMapper)
        {
            this.guidUserMapper = guidUserMapper;

            Get["/"] = x => View["index"];

            Get["/login"] = x =>
            {
                dynamic model = new ExpandoObject();
                model.Errored = Request.Query.error.HasValue;
                model.UserName = Request.Query.username.HasValue ? Request.Query.username.Value : "";

                return View["login", model];
            };

            Post["/login"] = HandleLogin;

            Get["/logout"] = x => this.LogoutAndRedirect("~/");
        }

        private Response HandleLogin(dynamic param)
        {
            var userGuid = guidUserMapper.ValidateUser((string)Request.Form.Username, (string)Request.Form.Password);

            if (userGuid == null)
            {
                return Context.GetRedirect("~/login?error=true&username=" + (string)Request.Form.Username);
            }

            DateTime? expiry = null;
            if (Request.Form.RememberMe.HasValue)
            {
                expiry = DateTime.Now.AddDays(7);
            }

            return this.LoginAndRedirect(userGuid.Value, expiry);
        }
    }
}
