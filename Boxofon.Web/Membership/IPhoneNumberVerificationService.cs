using System;

namespace Boxofon.Web.Membership
{
    public interface IPhoneNumberVerificationService
    {
        void BeginVerification(Guid userId, string phoneNumber);
        bool TryCompleteVerification(Guid userId, string phoneNumber, string code);
    }
}