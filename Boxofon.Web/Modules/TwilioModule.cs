using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Security;
using Boxofon.Web.Twilio;
using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using Twilio.Mvc;
using Twilio.TwiML;

namespace Boxofon.Web.Modules
{
    public class TwilioModule : NancyModule
    {
        private readonly IPhoneNumberBlacklist _phoneNumberBlacklist;
        private readonly IMailgunRestClient _mailgun;
        private readonly IUrlHelper _urlHelper;

        public TwilioModule(IPhoneNumberBlacklist phoneNumberBlacklist, IMailgunRestClient mailgun, IUrlHelper urlHelper)
            : base("/twilio")
        {
            this.RequiresWebhookAuthKey();

            // Verify that the request is done by Twilio.
            //Before += ctx => (new Boxofon.Web.Twilio.RequestValidator()).IsValidRequest(ctx, WebConfigurationManager.AppSettings["twilio:AuthToken"]) ?
            //                     null :
            //                     new Response { StatusCode = HttpStatusCode.Unauthorized };

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

            Get["/test"] = parameters =>
            {
                var response = new TwilioResponse();
                response.SayInSwedish("Hej hej!");
                return response.ToNancyResponse();
            };

            Post["/incoming"] = parameters =>
            {
                var request = this.Bind<VoiceRequest>();
                var response = new TwilioResponse();
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
                }

                if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
                {
                    try
                    {
                        _mailgun.SendMessage(
                            from: WebConfigurationManager.AppSettings["boxofon:NoreplyEmail"],
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
                }
                else
                {
                    response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
                }
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
                    _mailgun.SendMessage(
                        from: WebConfigurationManager.AppSettings["boxofon:NoreplyEmail"],
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
        }
    }
}