using System;
using Boxofon.Web.Helpers;
using Nancy;
using Nancy.Security;
using Nancy.Authentication.Forms;
using TinyMessenger;

namespace Boxofon.Web.Modules
{
    public class HomeModule : WebsiteBaseModule
    {
        public HomeModule()
        {
            Get["/"] = parameters =>
            {
                if (this.IsAuthenticated())
                {
                    return Response.AsRedirect("/account");
                }
                return View["Default.cshtml"];
            };

            Get["/home"] = parameters => View["Default.cshtml"];

            Get["/account/signin"] = parameters =>
            {
                if (this.IsAuthenticated())
                {
                    return Response.AsRedirect("/account");
                }
                return View["SignIn.cshtml"];
            };

            Get["/account/signout"] = parameters => this.LogoutAndRedirect("~/");
        }
    }
}