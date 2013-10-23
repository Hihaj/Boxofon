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
    public class VoiceController : TwilioController
    {
        [RequireWebhookAuthKey]
        [HttpPost]
        [ValidateTwilioRequest]
        public ActionResult Incoming(VoiceRequest request)
        {
            var response = new TwilioResponse();
            response.Say("Connecting you now.");
            response.Dial(WebConfigurationManager.AppSettings["MyPhoneNumber"]);
            return TwiML(response);
        }
    }
}
