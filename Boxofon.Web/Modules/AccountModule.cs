using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public class AccountModule : WebsiteBaseModule
    {
        public AccountModule() : base("/account")
        {
            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                return View["Overview.cshtml"];
            };
        }
    }
}