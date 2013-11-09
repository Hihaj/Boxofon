using System;

namespace Boxofon.Web.Membership
{
    public interface IExternalIdentityLookup
    {
        Guid? GetBoxofonUserId(string providerName, string providerUserId);
    }
}