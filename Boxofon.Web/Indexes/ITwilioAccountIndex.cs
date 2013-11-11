using System;

namespace Boxofon.Web.Indexes
{
    public interface ITwilioAccountIndex
    {
        Guid? GetBoxofonUserId(string twilioAccountSid);
    }
}