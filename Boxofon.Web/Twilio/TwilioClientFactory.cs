using System;
using System.Web.Configuration;
using Boxofon.Web.Membership;
using Twilio;

namespace Boxofon.Web.Twilio
{
    public class TwilioClientFactory : ITwilioClientFactory
    {
        public TwilioRestClient GetApplicationClient()
        {
            return new TwilioRestClient(WebConfigurationManager.AppSettings["twilio:AccountSid"], WebConfigurationManager.AppSettings["twilio:AuthToken"]);
        }

        public TwilioRestClient GetUserClient(User user)
        {
            if (string.IsNullOrEmpty(user.TwilioAccountSid))
            {
                throw new ArgumentException("The specified user does not have a Twilio AccountSid.");
            }
            return new TwilioRestClient(user.TwilioAccountSid, WebConfigurationManager.AppSettings["twilio:AuthToken"]);
        }
    }
}