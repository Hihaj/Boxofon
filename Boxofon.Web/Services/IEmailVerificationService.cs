using System;

namespace Boxofon.Web.Services
{
    public interface IEmailVerificationService
    {
        void BeginVerification(Guid userId, string email);
        bool TryCompleteVerification(Guid userId, string email, string code);
    }
}