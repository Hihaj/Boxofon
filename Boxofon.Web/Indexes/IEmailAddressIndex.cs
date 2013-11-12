using System;

namespace Boxofon.Web.Indexes
{
    public interface IEmailAddressIndex
    {
        Guid? GetBoxofonUserId(string email);
    }
}