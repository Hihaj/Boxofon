using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Membership;
using Boxofon.Web.Messages;
using Boxofon.Web.Security;
using Boxofon.Web.Twilio;
using NLog;
using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using Nancy.Security;
using TinyMessenger;
using Twilio.Mvc;
using Twilio.TwiML;

namespace Boxofon.Web.Modules
{
    public class TwilioModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IPhoneNumberBlacklist _phoneNumberBlacklist;
        private readonly IMailgunClient _mailgun;
        private readonly IUrlHelper _urlHelper;
        private readonly IUserRepository _userRepository;
        private readonly ITwilioAccountLookup _twilioAccountLookup;
        private readonly ITinyMessengerHub _hub;

        public TwilioModule(
            IPhoneNumberBlacklist phoneNumberBlacklist, 
            IMailgunClient mailgun, 
            IUrlHelper urlHelper, 
            IUserRepository userRepository, 
            ITwilioAccountLookup twilioAccountLookup, 
            ITinyMessengerHub hub)
            : base("/twilio")
        {
            _phoneNumberBlacklist = phoneNumberBlacklist;
            if (mailgun == null)
            {
                throw new ArgumentNullException("mailgun");
            }
            _mailgun = mailgun;
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
            if (twilioAccountLookup == null)
            {
                throw new ArgumentNullException("twilioAccountLookup");
            }
            _twilioAccountLookup = twilioAccountLookup;
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;
            
            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            this.RequiresValidTwilioSignature();

            Post["/incoming"] = parameters =>
            {
                var request = this.Bind<VoiceRequest>();
                var response = new TwilioResponse();

                // Let the owner dial an arbitrary number.
                if (request.From == WebConfigurationManager.AppSettings["MyPhoneNumber"])
                {
                    response.BeginGather(new
                    {
                        action = _urlHelper.GetAbsoluteUrl("/twilio/outgoing", new Dictionary<string, string>() { { "authKey", WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"] } }),
                        method = "POST",
                        timeout = 5,
                        finishOnKey = "#"
                    });
                    response.SayInSwedish("Välkommen! Skriv in det telefonnummer du vill ringa. Avsluta med fyrkant.");
                    response.EndGather();
                    return response.ToNancyResponse();
                }

                // Block blacklisted numbers (with the option of recording a message).
                if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
                {
                    try
                    {
                        _mailgun.SendNoReplyMessage(
                            to: WebConfigurationManager.AppSettings["MyEmail"],
                            subject: string.Format("Blockerat samtal från {0}", request.From),
                            htmlBody: string.Format(@"Din Boxofon blockerade just ett samtal från <a href=""http://vemringde.se/?q={0}"">{1}</a>.", HttpUtility.UrlEncode(request.From), request.From));
                    }
                    catch (Exception ex)
                    {
                        // TODO Handle error (log?) - see https://github.com/ServiceStack/ServiceStack/wiki/Http-Utils#exception-handling
                    }

                    response.SayInSwedish("Du ringer från ett svartlistat nummer och kommer inte kopplas fram. Om du vill kan du lämna ett meddelande efter tonen.");
                    response.Record(new
                    {
                        action = _urlHelper.GetAbsoluteUrl("/twilio/voicemail", new Dictionary<string, string>() { { "authKey", WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"] } }),
                        method = "POST",
                        timeout = 5,
                        maxLength = 180,
                        playBeep = true,
                        finishOnKey = "#"
                    });
                    return response.ToNancyResponse();
                }

                // Forward call to owner.
                response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
                return response.ToNancyResponse();
            };

            Post["/outgoing"] = parameters =>
            {
                var request = this.Bind<VoiceRequest>();
                string numberToCall = null;
                var response = new TwilioResponse();
                if (request.From == WebConfigurationManager.AppSettings["MyPhoneNumber"] &&
                    !string.IsNullOrEmpty(request.Digits))
                {
                    if (request.Digits.StartsWith("00"))
                    {
                        numberToCall = "+" + request.Digits.Remove(0, 2);
                    }
                    else if (request.Digits.StartsWith("0"))
                    {
                        numberToCall = "+46" + request.Digits.Remove(0, 1);
                    }
                    else
                    {
                        numberToCall = "+46" + request.Digits;
                    }
                }
                if (!string.IsNullOrEmpty(numberToCall))
                {
                    response.Dial(numberToCall, new
                    {
                        callerId = request.To
                    });
                }
                else
                {
                    response.Hangup();
                }
                return response.ToNancyResponse();
            };

            Post["/voicemail"] = parameters =>
            {
                var request = this.Bind<VoiceRequest>();
                try
                {
                    _mailgun.SendNoReplyMessage(
                        to: WebConfigurationManager.AppSettings["MyEmail"],
                        subject: string.Format("Nytt röstmeddelande från {0}", request.From),
                        htmlBody: string.Format(@"Du har ett nytt röstmeddelande från {0}. <a href=""{1}.mp3"">Klicka här för att lyssna.</a>", request.From, request.RecordingUrl));
                }
                catch (Exception ex)
                {
                    // TODO Handle error (log?) - see https://github.com/ServiceStack/ServiceStack/wiki/Http-Utils#exception-handling
                }

                var response = new TwilioResponse();
                response.SayInSwedish("Tack för ditt samtal.");
                response.Hangup();
                return response.ToNancyResponse();
            };

            Post["/connect/deauthorize"] = parameters =>
            {
                var twilioUserAccountSid = Request.Form["AccountSid"];
                var boxofonConnectAppSid = Request.Form["ConnectAppSid"];

                if (boxofonConnectAppSid != WebConfigurationManager.AppSettings["twilio:ConnectAppSid"])
                {
                    Logger.Info("Received a Twilio Connect deauthorization request with the wrong ConnectAppSid ('{0}').", boxofonConnectAppSid);
                    return HttpStatusCode.BadRequest;
                }

                if (string.IsNullOrEmpty(twilioUserAccountSid))
                {
                    Logger.Info("Received a Twilio Connect deauthorization request without AccountSid.");
                    return HttpStatusCode.BadRequest;
                }

                User user = null;
                var userId = _twilioAccountLookup.GetBoxofonUserId((string)twilioUserAccountSid);
                if (userId.HasValue)
                {
                    user = _userRepository.GetById(userId.Value);
                }
                if (user == null)
                {
                    Logger.Info("Received a Twilio Connect deauthorization request for a user that does not exist (AccountSid = '{0}').", twilioUserAccountSid);
                    return HttpStatusCode.OK;
                }

                Logger.Debug("Received a Twilio Connect deauthorization request for user '{0}'.", user.Id);
                user.TwilioAccountSid = null;
                _userRepository.Save(user);
                _hub.PublishAsync(new UnlinkedTwilioAccountFromUser
                {
                    TwilioAccountSid = (string)twilioUserAccountSid,
                    UserId = userId.Value
                });
                return HttpStatusCode.OK;
            };
        }
    }
}