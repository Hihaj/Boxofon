using System;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public abstract class ExternalIdentityLookupBase : IExternalIdentityLookup
    {
        protected readonly ITinyMessengerHub Hub;

        protected ExternalIdentityLookupBase(ITinyMessengerHub hub)
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            Hub = hub;

            // Set up subscriptions
            Hub.Subscribe<UserCreated>(msg => AddExternalIdentity(msg.ExternalIdentity.ProviderName, msg.ExternalIdentity.ProviderUserId, msg.UserId));
            Hub.Subscribe<AddedExternalIdentityToUser>(msg => AddExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId));
        }

        public abstract Guid? GetBoxofonUserId(string providerName, string providerUserId);
        protected abstract void AddExternalIdentity(string providerName, string providerUserId, Guid userId);
    }
}