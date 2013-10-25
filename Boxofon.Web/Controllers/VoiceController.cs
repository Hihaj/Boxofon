using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Boxofon.Web.Filters;
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
            return new TwiMLResult(response);
            
            //var response = new TwilioResponse();
            //if (request.From == WebConfigurationManager.AppSettings["MyPhoneNumber"])
            //{
            //    response.Say("Välkommen!", new { voice = "alice", language = "sv-SE" });
            //}

            //if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
            //{
            //    response.Say("Du ringer från ett svartlistat nummer och kommer inte kopplas fram. Om du vill kan du lämna ett meddelande efter tonen.", new { voice = "alice", language = "sv-SE" });
            //    response.Record(new
            //    {
            //        action = Url.Action("VoiceMail", "Voice", new { authKey = WebConfigurationManager.AppSettings["WebhookAuthKey"] }),
            //        method = "POST",
            //        timeout = 5,
            //        maxLength = 180,
            //        playBeep = true,
            //        finishOnKey = "#"
            //    });
            //}
            //else
            //{
            //    response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
            //}
            //return new TwiMLResult(response);
        }

        [RequireWebhookAuthKey]
        [HttpPost]
        [ValidateTwilioRequest]
        public ActionResult VoiceMail(VoiceRequest request)
        {
            // TODO Send an e-mail to the user saying that there is a new voicemail.
            var response = new TwilioResponse();
            response.Say("Tack för ditt samtal.");
            response.Hangup();
            return new TwiMLResult(response);
        }
    }
}
