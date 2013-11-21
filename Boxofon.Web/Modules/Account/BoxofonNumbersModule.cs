using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Messages;
using Boxofon.Web.Model;
using Boxofon.Web.Security;
using Boxofon.Web.Twilio;
using Boxofon.Web.ViewModels;
using NLog;
using Nancy;
using Nancy.Security;
using TinyMessenger;
using Twilio;

namespace Boxofon.Web.Modules.Account
{
    public class BoxofonNumbersModule : WebsiteBaseModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITwilioClientFactory _twilioClientFactory;
        private readonly IUrlHelper _urlHelper;
        private readonly IUserRepository _userRepository;
        private readonly ITwilioPhoneNumberRepository _twilioPhoneNumberRepository;
        private readonly ITinyMessengerHub _hub;

        public BoxofonNumbersModule(
            ITwilioClientFactory twilioClientFactory,
            IUrlHelper urlHelper,
            IUserRepository userRepository,
            ITwilioPhoneNumberRepository twilioPhoneNumberRepository,
            ITinyMessengerHub hub)
            : base("/account/numbers/boxofon")
        {
            if (twilioClientFactory == null)
            {
                throw new ArgumentNullException("twilioClientFactory");
            }
            _twilioClientFactory = twilioClientFactory;
            if (urlHelper == null)
            {
                throw new ArgumentNullException("urlHelper");
            }
            _urlHelper = urlHelper;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            if (twilioPhoneNumberRepository == null)
            {
                throw new ArgumentNullException("twilioPhoneNumberRepository");
            }
            _twilioPhoneNumberRepository = twilioPhoneNumberRepository;
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;

            this.RequiresAuthentication();

            // TODO Make async
            Get["/"] = parameters =>
            {
                var twilio = _twilioClientFactory.GetClientForUser(this.GetCurrentUser());
                var availableNumbers = twilio.ListAvailableMobilePhoneNumbers("SE", new AvailablePhoneNumberListRequest());
                var viewModel = new ViewModels.AvailableBoxofonNumbers
                {
                    Numbers = availableNumbers.AvailablePhoneNumbers.Select(number => new BoxofonNumber
                    {
                        FriendlyName = number.FriendlyName,
                        PhoneNumber = number.PhoneNumber
                    }).ToArray()
                };
                return View["Index.cshtml", viewModel];
            };

            // TODO Make async
            Post["/"] = parameters =>
            {
                var phoneNumber = (string)Request.Form.phoneNumber;
                if (!phoneNumber.IsPossiblyValidPhoneNumber())
                {
                    Request.AddAlertMessage("error", "Välj ett telefonnummer du vill köpa.");
                    return Response.AsRedirect("/account/numbers/boxofon");
                }
                var twilio = _twilioClientFactory.GetClientForUser(this.GetCurrentUser());
                var result = twilio.AddIncomingPhoneNumber(new PhoneNumberOptions
                {
                    PhoneNumber = phoneNumber,
                    VoiceUrl = _urlHelper.GetAbsoluteUrl("/twilio/voice/incoming", new Dictionary<string, string> { { "authKey", WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"] } }),
                    VoiceMethod = "POST",
                    SmsUrl = _urlHelper.GetAbsoluteUrl("/twilio/sms/incoming", new Dictionary<string, string> { { "authKey", WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"] } }),
                    SmsMethod = "POST"
                });
                // TODO Handle REST exception in a prettier way.
                if (result.RestException != null)
                {
                    switch (result.RestException.Code)
                    {
                        case "21421":
                            Request.AddAlertMessage("error", "Ogiltigt telefonnummer. Köpet har avbrutits.");
                            break;

                        case "21422":
                            Request.AddAlertMessage("error", "Det valda telefonnumret är inte tillgängligt. Köpet har avbrutits.");
                            break;

                        default:
                            Logger.Error("Error purchasing phone number '{0}' from Twilio ('{1}' - code {2}).", phoneNumber, result.RestException.Message, result.RestException.Code);
                            Request.AddAlertMessage("error", "Ett fel uppstod i kommunikationen med Twilio. Köpet har avbrutits.");
                            break;
                    }
                    return Response.AsRedirect("/account/numbers/boxofon");
                }

                // Update the user
                var user = this.GetCurrentUser();
                user.TwilioPhoneNumber = result.PhoneNumber;
                _userRepository.Save(user);

                // Save the phone number in Boxofon
                var twilioNumber = new TwilioPhoneNumber
                {
                    PhoneNumber = result.PhoneNumber,
                    FriendlyName = result.FriendlyName,
                    TwilioAccountSid = result.AccountSid,
                    UserId = user.Id
                };
                _twilioPhoneNumberRepository.Save(twilioNumber);

                _hub.PublishAsync(new AllocatedTwilioPhoneNumber
                {
                    PhoneNumber = result.PhoneNumber,
                    FriendlyName = result.FriendlyName,
                    TwilioAccountSid = result.AccountSid,
                    UserId = user.Id
                });

                Request.AddAlertMessage("success", string.Format("Grattis! Du äger nu Boxofonnumret {0}.", result.FriendlyName));
                return Response.AsRedirect("/account");
            };
        }
    }
}