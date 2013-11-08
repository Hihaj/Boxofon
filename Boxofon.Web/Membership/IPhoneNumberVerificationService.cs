using System;

namespace Boxofon.Web.Membership
{
    public interface IPhoneNumberVerificationService
    {
        void BeginPhoneNumberVerification(Guid userId, string phoneNumber);
        bool TryCompletePhoneNumberVerification(Guid userId, string phoneNumber, string code);
    }
}