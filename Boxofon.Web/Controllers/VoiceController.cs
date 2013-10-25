using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Boxofon.Web.Filters;
using Boxofon.Web.Twilio;
using ServiceStack.Text;
using Twilio.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace Boxofon.Web.Controllers
{
    public class VoiceController : Controller
    {
        private readonly IPhoneNumberBlacklist _phoneNumberBlacklist;

        public VoiceController(IPhoneNumberBlacklist phoneNumberBlacklist)
        {
            _phoneNumberBlacklist = phoneNumberBlacklist;
        }

        [RequireWebhookAuthKey]
        [HttpPost]
        [ValidateTwilioRequest]
        public ActionResult Incoming(VoiceRequest request)
        {
            var response = new TwilioResponse();
            if (request.From == WebConfigurationManager.AppSettings["MyPhoneNumber"])
            {
                response.Say("Välkommen!", new { voice = "alice", language = "sv-SE" });
            }

            if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
            {
                response.Say("Du ringer från ett svartlistat nummer och kommer inte kopplas fram. Om du vill kan du lämna ett meddelande efter tonen.", new { voice = "alice", language = "sv-SE" });
                response.Record(new
                {
                    action = Url.Action("VoiceMail", "Voice", new { authKey = WebConfigurationManager.AppSettings["WebhookAuthKey"] }),
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
            return new TwiMLResult(response);
        }

        [RequireWebhookAuthKey]
        [HttpPost]
        [ValidateTwilioRequest]
        public ActionResult VoiceMail(VoiceRequest request)
        {
            try
            {
                "https://api.mailgun.net/v2/{0}/messages"
                    .Fmt(WebConfigurationManager.AppSettings["mailgun:Domain"])
                    .PostToUrl(new
                    {
                        from = WebConfigurationManager.AppSettings["BoxofonNoreplyEmail"],
                        to = WebConfigurationManager.AppSettings["MyEmail"],
                        subject = string.Format("Nytt röstmeddelande från {0}", request.From),
                        html = string.Format(@"Du har ett nytt röstmeddelande från {0}. <a href=""{1}.mp3"">Klicka här för att lyssna.</a>", request.From, request.RecordingUrl)
                    },
                    requestFilter: webRequest => { webRequest.Credentials = new NetworkCredential("api", WebConfigurationManager.AppSettings["mailgun:ApiKey"]); });
            }
            catch (Exception ex)
            {
                // TODO Handle error (log?) - see https://github.com/ServiceStack/ServiceStack/wiki/Http-Utils#exception-handling
            }

            var response = new TwilioResponse();
            response.SayInSwedish("Tack för ditt samtal.");
            response.Hangup();
            return new TwiMLResult(response);
        }

        [RequireWebhookAuthKey]
        [HttpPost]
        [ValidateTwilioRequest]
        public ActionResult Outgoing(VoiceRequest request)
        {
            return new ActionResults.TwiMLResult(new TwilioResponse());
        }
    }
}
