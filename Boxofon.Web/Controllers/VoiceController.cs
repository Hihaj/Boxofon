using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Boxofon.Web.Filters;
using Twilio.Mvc;
using Twilio.TwiML;

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
                response.Say("Välkommen!", new { language = "sv-SE" });
            }

            if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
            {
                response.Say("Du ringer från ett svartlistat nummer och kommer inte kopplas fram. Om du vill kan du lämna ett meddelande efter tonen.", new { language = "sv-SE" });
                response.Hangup();
            }
            else
            {
                response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
            }
            return new ActionResults.TwiMLResult(response);
        }
    }
}
