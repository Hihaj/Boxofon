using System;
using System.Linq;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Messages;
using Boxofon.Web.Model;
using Boxofon.Web.Twilio;
using NLog;
using Nancy;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules.Account
{
    public class DashboardModule : WebsiteBaseModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITinyMessengerHub _hub;
        private readonly IUserRepository _userRepository;
        private readonly ITwilioClientFactory _twilioClientFactory;
        private readonly ITwilioPhoneNumberRepository _twilioPhoneNumberRepository;

        public DashboardModule(
            ITinyMessengerHub hub, 
            IUserRepository userRepository,
            ITwilioClientFactory twilioClientFactory,
            ITwilioPhoneNumberRepository twilioPhoneNumberRepository) : base("/account")
        {
            hub.ThrowIfNull("hub");
            userRepository.ThrowIfNull("userRepository");
            twilioClientFactory.ThrowIfNull("twilioClientFactory");
            twilioPhoneNumberRepository.ThrowIfNull("twilioPhoneNumberRepository");
            _hub = hub;
            _userRepository = userRepository;
            _twilioClientFactory = twilioClientFactory;
            _twilioPhoneNumberRepository = twilioPhoneNumberRepository;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                var viewModel = new ViewModels.AccountOverview
                {
                    IsTwilioAccountConnected = !string.IsNullOrEmpty(user.TwilioAccountSid),
                    TwilioConnectAuthorizationUrl = string.Format("https://www.twilio.com/authorize/{0}", WebConfigurationManager.AppSettings["twilio:ConnectAppSid"]),
                    TwilioAccountManagementUrl = string.IsNullOrEmpty(user.TwilioAccountSid) ? null : string.Format("https://www.twilio.com/user/account/usage/authorized-app/{0}", WebConfigurationManager.AppSettings["twilio:ConnectAppSid"]),
                    BoxofonNumber = user.TwilioPhoneNumber,
                    PrivatePhoneNumber = user.PrivatePhoneNumber,
                    Email = user.Email
                };
                return View["Dashboard.cshtml", viewModel];
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
                _hub.PublishAsync(new LinkedTwilioAccountToUser
                {
                    TwilioAccountSid = twilioAccountSid,
                    UserId = user.Id
                });

                // Fetch already owned Twilio numbers managed by Boxofon.
                // TODO Handle multiple existing numbers
                try
                {
                    var twilio = _twilioClientFactory.GetClientForUser(user);
                    var twilioPhoneNumber = twilio.ListIncomingMobilePhoneNumbers().IncomingPhoneNumbers.Select(number => new TwilioPhoneNumber
                    {
                        UserId = user.Id,
                        TwilioAccountSid = user.TwilioAccountSid,
                        PhoneNumber = number.PhoneNumber.ToE164(),
                        FriendlyName = number.FriendlyName
                    }).FirstOrDefault();
                    if (twilioPhoneNumber != null)
                    {
                        _twilioPhoneNumberRepository.Save(twilioPhoneNumber);
                        _hub.PublishAsync(new AllocatedTwilioPhoneNumber
                        {
                            PhoneNumber = twilioPhoneNumber.PhoneNumber,
                            FriendlyName = twilioPhoneNumber.FriendlyName,
                            TwilioAccountSid = user.TwilioAccountSid,
                            UserId = user.Id
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(string.Format("Error fetching Twilio phone numbers for user '{0}'.", user.Id), ex);
                }

                
                Request.AddAlertMessage("success", "Twilio-kontot har anslutits.");
                return Response.AsRedirect("/account");
            };
        }
    }
}