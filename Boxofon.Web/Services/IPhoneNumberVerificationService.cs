using System;
using Boxofon.Web.Model;

namespace Boxofon.Web.Services
{
    public interface IPhoneNumberVerificationService
    {
        void BeginVerification(User user, string phoneNumber);
        bool TryCompleteVerification(User user, string phoneNumber, string code);
    }
}