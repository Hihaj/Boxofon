using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Indexes;
using Boxofon.Web.Messages;
using Boxofon.Web.Model;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.Authentication.Forms;
using TinyMessenger;

namespace Boxofon.Web.Security
{
    public class AuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        private readonly IExternalIdentityIndex _externalIdentityIndex;
        private readonly IUserRepository _userRepository;
        private readonly ITinyMessengerHub _hub;

        public AuthenticationCallbackProvider(IExternalIdentityIndex externalIdentityIndex, IUserRepository userRepository, ITinyMessengerHub hub)
        {
            if (externalIdentityIndex == null)
            {
                throw new ArgumentNullException("externalIdentityIndex");
            }
            _externalIdentityIndex = externalIdentityIndex;

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
            var userId = _externalIdentityIndex.GetBoxofonUserId(model.AuthenticatedClient.ProviderName, model.AuthenticatedClient.UserInformation.Id);
            if (nancyModule.IsAuthenticated())
            {
                if (!userId.HasValue)
                {
                    // Link provider identity to signed in user
                    var user = nancyModule.GetCurrentUser();
                    var providerId = new ExternalIdentity
                    {
                        ProviderName = model.AuthenticatedClient.ProviderName, 
                        ProviderUserId = model.AuthenticatedClient.UserInformation.Id
                    };
                    user.ExternalIdentities.Add(providerId);
                    _userRepository.Save(user);
                    _hub.PublishAsync(new AddedExternalIdentityToUser
                    {
                        ProviderName = providerId.ProviderName,
                        ProviderUserId = providerId.ProviderUserId,
                        UserId = user.Id
                    });
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
                        //Email = model.AuthenticatedClient.UserInformation.Email,
                    };
                    var externalId = new ExternalIdentity
                    {
                        ProviderName = model.AuthenticatedClient.ProviderName, 
                        ProviderUserId = model.AuthenticatedClient.UserInformation.Id
                    };
                    user.ExternalIdentities.Add(externalId);
                    _userRepository.Save(user);
                    _hub.PublishAsync(new UserCreated
                    {
                        UserId = user.Id,
                        ExternalIdentity = externalId
                    });
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