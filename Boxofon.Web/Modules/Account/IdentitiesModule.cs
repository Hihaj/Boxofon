using System;
using System.Linq;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Messages;
using Boxofon.Web.Model;
using Nancy;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules.Account
{
    public class IdentitiesModule : WebsiteBaseModule
    {
        private readonly ITinyMessengerHub _hub;
        private readonly IUserRepository _userRepository;

        public IdentitiesModule(
            ITinyMessengerHub hub, 
            IUserRepository userRepository)
            : base("/account/identities")
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                var viewModel = new ViewModels.ExternalIdentities
                {
                    AllowUnlinkIds = user.ExternalIdentities.Count > 1,
                    GoogleIdLinked = user.ExternalIdentities.Any(id => id.ProviderName == "google"),
                    TwitterIdLinked = user.ExternalIdentities.Any(id => id.ProviderName == "twitter"),
                    FacebookIdLinked = user.ExternalIdentities.Any(id => id.ProviderName == "facebook"),
                    WindowsLiveIdLinked = user.ExternalIdentities.Any(id => id.ProviderName == "windowslive")
                };
                return View["Account/Identities.cshtml", viewModel];
            };

            Post["/{providerName}"] = parameters =>
            {
                var op = (string)Request.Form.operation;
                if (op == "delete")
                {
                    var user = this.GetCurrentUser();
                    if (user.ExternalIdentities.Count <= 1)
                    {
                        Request.AddAlertMessage("error", "Du måste ha kvar åtminstone ett inloggningssätt.");
                        return Response.AsRedirect("/account/identities");
                    }
                    var extId = user.ExternalIdentities.FirstOrDefault(id => id.ProviderName == (string)parameters.providerName);
                    if (extId != null)
                    {
                        user.ExternalIdentities.Remove(extId);
                        _userRepository.Save(user);
                        _hub.Publish(new RemovedExternalIdentityFromUser
                        {
                            ProviderName = extId.ProviderName,
                            ProviderUserId = extId.ProviderUserId,
                            UserId = user.Id
                        });
                        Request.AddAlertMessage("success", "Inloggningssättet togs bort.");
                    }
                }
                return Response.AsRedirect("/account/identities");
            };
        }
    }
}