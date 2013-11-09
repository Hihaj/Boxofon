using System;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public abstract class ExternalIdentityLookupBase : IExternalIdentityLookup, ISubscriber
    {
        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            hub.Subscribe<UserCreated>(msg => AddExternalIdentity(msg.ExternalIdentity.ProviderName, msg.ExternalIdentity.ProviderUserId, msg.UserId));
            hub.Subscribe<AddedExternalIdentityToUser>(msg => AddExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId));
        }

        public abstract Guid? GetBoxofonUserId(string providerName, string providerUserId);
        protected abstract void AddExternalIdentity(string providerName, string providerUserId, Guid userId);
    }
}