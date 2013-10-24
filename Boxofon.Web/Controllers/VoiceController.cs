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
            if (_phoneNumberBlacklist != null && _phoneNumberBlacklist.Contains(request.From))
            {
                response.Say("You are calling from a blacklisted number. Goodbye.");
                response.Hangup();
            }
            else
            {
                response.Say("Connecting you now.");
                response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
            }
            return new ActionResults.TwiMLResult(response);
        }
    }
}
