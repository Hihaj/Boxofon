using System;

namespace Boxofon.Web.Membership
{
    public interface IExternalIdentityService
    {
        Guid? GetBoxofonUserId(string providerName, string providerUserId);
    }
}