using System;

namespace Boxofon.Web.Twilio
{
    public interface ITwilioAccountService
    {
        Guid? GetBoxofonUserId(string twilioAccountSid);
    }
}