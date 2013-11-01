using System;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Membership;
using Boxofon.Web.ViewModels;
using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public class AccountModule : WebsiteBaseModule
    {
        private readonly IUserRepository _userRepository;

        public AccountModule(IUserRepository userRepository) : base("/account")
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                var viewModel = new AccountOverviewViewModel
                {
                    IsTwilioAccountConnected = !string.IsNullOrEmpty(user.TwilioAccountSid),
                    TwilioConnectAuthorizationUrl = string.Format("https://www.twilio.com/authorize/{0}", WebConfigurationManager.AppSettings["twilio:ConnectAppSid"])
                };
                return View["Overview.cshtml", viewModel];
            };

            Get["/twilio/connect/authorize"] = parameters =>
            {
                var twilioAccountSid = Request.Query["AccountSid"];
                if (Request.Query["error"] == "unauthorized_client" || string.IsNullOrEmpty(twilioAccountSid))
                {
                    Request.AddAlertMessage("info", "Anslutning till Twilio-konto avbröts eller misslyckades.");
                    return Response.AsRedirect("/account");
                }

                var user = this.GetCurrentUser();
                if (!string.IsNullOrEmpty(user.TwilioAccountSid))
                {
                    Request.AddAlertMessage("error", "Du har redan anslutit ett Twilio-konto.");
                    return Response.AsRedirect("/account");
                }

                user.TwilioAccountSid = twilioAccountSid;
                _userRepository.Save(user);
                Request.AddAlertMessage("success", "Twilio-kontot har anslutits.");
                return Response.AsRedirect("/account");
            };
        }
    }
}