using Twilio.TwiML;

namespace Boxofon.Web.Twilio
{
    public static class TwilioResponseExtensions
    {
        public static TwilioResponse SayInSwedish(this TwilioResponse response, string text)
        {
            return response.Say(text, new { voice = "alice", language = "sv-SE" });
        }
    }
}