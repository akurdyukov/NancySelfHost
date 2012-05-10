using Nancy;
using Nancy.Security;
using NancySelfHost.Models;

namespace NancySelfHost
{
    public class SecureModule : NancyModule
    {
        public SecureModule()
            : base("/secure")
        {
            this.RequiresAuthentication();

            Get["/"] = x =>
                           {
                               UserIndentity identity = (UserIndentity) Context.CurrentUser;
                               var model = new UserModel(identity.UserName, identity.AuthId);
                               return View["secure", model];
                           };
        }
    }
}
