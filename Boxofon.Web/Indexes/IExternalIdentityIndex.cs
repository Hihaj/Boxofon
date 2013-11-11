using System;

namespace Boxofon.Web.Indexes
{
    public interface IExternalIdentityIndex
    {
        Guid? GetBoxofonUserId(string providerName, string providerUserId);
    }
}