using Boxofon.Web.Model;
using Twilio;

namespace Boxofon.Web.Twilio
{
    public interface ITwilioClientFactory
    {
        TwilioRestClient GetClientForApplication();
        TwilioRestClient GetClientForUser(User user);
    }
}