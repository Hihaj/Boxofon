using Boxofon.Web.Membership;
using Twilio;

namespace Boxofon.Web.Twilio
{
    public interface ITwilioClientFactory
    {
        TwilioRestClient GetApplicationClient();
        TwilioRestClient GetUserClient(User user);
    }
}