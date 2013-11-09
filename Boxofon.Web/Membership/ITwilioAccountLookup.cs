using System;

namespace Boxofon.Web.Membership
{
    public interface ITwilioAccountLookup
    {
        Guid? GetBoxofonUserId(string twilioAccountSid);
    }
}