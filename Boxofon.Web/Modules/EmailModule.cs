using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Model;
using Boxofon.Web.Services;
using Nancy;
using Nancy.Helpers;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules
{
    public class EmailModule : WebsiteBaseModule
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IUserRepository _userRepository;
        private readonly ITinyMessengerHub _hub;

        public EmailModule(IEmailVerificationService emailVerificationService, IUserRepository userRepository, ITinyMessengerHub hub) : base("/account/email")
        {
            if (emailVerificationService == null)
            {
                throw new ArgumentNullException("emailVerificationService");
            }
            _emailVerificationService = emailVerificationService;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                if (!string.IsNullOrEmpty(user.Email))
                {
                    Request.AddAlertMessage("error", "Det finns redan en e-postadress angiven. Ta bort den först om du vill använda en annan.");
                    return Response.AsRedirect("/account");
                }
                return View["Index.cshtml"];
            };

            Post["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                if (!string.IsNullOrEmpty(user.Email))
                {
                    Request.AddAlertMessage("error", "Det finns redan en e-postadress angiven. Ta bort den först om du vill använda en annan.");
                    return Response.AsRedirect("/account");
                }
                var email = (string)Request.Form.email;
                if (string.IsNullOrEmpty(email))
                {
                    Request.AddAlertMessage("error", "Du måste ange en e-postadress.");
                    return View["Index.cshtml"];
                }
                if (!email.IsPossiblyValidEmail())
                {
                    Request.AddAlertMessage("error", "E-postadressen ser ut att vara ogiltig. Kontrollera att du skrivit rätt.");
                    ViewBag.Email = email;
                    return View["Index.cshtml"];
                }

                _emailVerificationService.BeginVerification(user.Id, email);
                return Response.AsRedirect(string.Format("/account/email/{0}/verification", email.ZBase32Encode()));
            };

            Get["/{emailZBase32}/verification"] = parameters =>
            {
                return View["Verification.cshtml"];
            };

            Post["/{emailZBase32}/verification"] = parameters =>
            {
                var user = this.GetCurrentUser();
                var email = ((string)parameters.emailZBase32).ZBase32Decode();
                var code = (string)Request.Form.code;
                var verificationSucceeded = _emailVerificationService.TryCompleteVerification(user.Id, email, code);
                if (!verificationSucceeded)
                {
                    Request.AddAlertMessage("error", "Felaktig verifieringskod.");
                    return View["Verification.cshtml"];
                }
                user.Email = email;
                _userRepository.Save(user);
                _hub.Publish(new Messages.AddedEmailToUser
                {
                    Email = email,
                    UserId = user.Id
                });
                Request.AddAlertMessage("success", "Din e-postadress är nu bekräftad.");
                return Response.AsRedirect("/account");
            };

            // TODO Remove e-mail?
        }
    }
}