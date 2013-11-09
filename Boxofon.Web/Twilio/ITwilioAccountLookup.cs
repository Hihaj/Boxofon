using System;

namespace Boxofon.Web.Twilio
{
    public interface ITwilioAccountLookup
    {
        Guid? GetBoxofonUserId(string twilioAccountSid);
    }
}