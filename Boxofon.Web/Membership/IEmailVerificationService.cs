using System;

namespace Boxofon.Web.Membership
{
    public interface IEmailVerificationService
    {
        void BeginVerification(Guid userId, string email);
        bool TryCompleteVerification(Guid userId, string email, string code);
    }
}