using System.Web.Configuration;
using Twilio.TwiML.Mvc;

namespace Boxofon.Web.Filters
{
    public class ValidateTwilioRequestAttribute : ValidateRequestAttribute
    {
        public ValidateTwilioRequestAttribute() : base(WebConfigurationManager.AppSettings["twilio:AuthToken"])
        {
        }
    }
}