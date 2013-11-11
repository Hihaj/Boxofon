using System;

namespace Boxofon.Web.Services
{
    public interface IPhoneNumberVerificationService
    {
        void BeginVerification(Guid userId, string phoneNumber);
        bool TryCompleteVerification(Guid userId, string phoneNumber, string code);
    }
}