using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Membership;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.Authentication.Forms;

namespace Boxofon.Web.Security
{
    public class AuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationCallbackProvider(IUserRepository userRepository)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new NotImplementedException();
        }

        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            if (model.Exception != null)
            {
                // TODO
                throw new NotImplementedException("Login failure", model.Exception);
            }
            var returnUrl = model.ReturnUrl ?? "/account";
            if (returnUrl.Contains("/authentication/redirect/"))
            {
                returnUrl = "/account";
            }
            var userId = _userRepository.GetIdByProviderNameAndProviderUserId(model.AuthenticatedClient.ProviderName, model.AuthenticatedClient.UserInformation.Id);
            if (nancyModule.IsAuthenticated())
            {
                if (!userId.HasValue)
                {
                    // Link provider identity to signed in user
                    var user = nancyModule.GetCurrentUser();
                    user.ProviderIdentities.Add(new User.ProviderIdentity { ProviderName = model.AuthenticatedClient.ProviderName, ProviderUserId = model.AuthenticatedClient.UserInformation.Id });
                    _userRepository.Save(user);
                    nancyModule.Request.AddAlertMessage("success", "Inloggningssättet lades till ditt konto.");
                    return nancyModule.Response.AsRedirect(returnUrl);
                }
                else
                {
                    if (nancyModule.GetCurrentUser().Id != userId.Value)
                    {
                        // Cannot link provider identity of another user to the currently signed in user.
                        // TODO
                        nancyModule.Request.AddAlertMessage("error", "Denna identitet är redan registrerad av en annan användare.");
                        return nancyModule.Response.AsRedirect(returnUrl);
                    }
                    else
                    {
                        // Provider identity already linked to the currently signed in user.
                        nancyModule.Request.AddAlertMessage("info", "Identiteten är redan registrerad på ditt konto.");
                        return nancyModule.Response.AsRedirect(returnUrl);
                    }
                }
            }
            else
            {
                if (!userId.HasValue)
                {
                    // Add new user.
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = model.AuthenticatedClient.UserInformation.Email,
                    };
                    user.ProviderIdentities.Add(new User.ProviderIdentity { ProviderName = model.AuthenticatedClient.ProviderName, ProviderUserId = model.AuthenticatedClient.UserInformation.Id });
                    _userRepository.Save(user);
                    nancyModule.Request.AddAlertMessage("success", "Välkommen som ny användare!");
                    return nancyModule.LoginAndRedirect(user.Id, fallbackRedirectUrl: returnUrl);
                }
                else
                {
                    // Sign in existing user.
                    nancyModule.Request.AddAlertMessage("success", "Du är nu inloggad. Välkommen!");
                    return nancyModule.LoginAndRedirect(userId.Value, fallbackRedirectUrl: returnUrl);
                }
            }
        }
    }
}