using System;
using System.Collections.Generic;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public class InMemoryExternalIdentityService : IExternalIdentityService
    {
        private readonly Dictionary<string, Guid>  _idLookup = new Dictionary<string, Guid>();
        private readonly ITinyMessengerHub _hub;

        public InMemoryExternalIdentityService(ITinyMessengerHub hub)
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;

            _hub.Subscribe<UserCreated>(msg => AddExternalIdentity(msg.ExternalIdentity.ProviderName, msg.ExternalIdentity.ProviderUserId, msg.UserId));
            _hub.Subscribe<AddedExternalIdentityToUser>(msg => AddExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId));
        }

        public Guid? GetBoxofonUserId(string providerName, string providerUserId)
        {
            Guid userId;
            if (_idLookup.TryGetValue(string.Format("{0}:{1}", providerName, providerUserId), out userId))
            {
                return userId;
            }
            return null;
        }

        private void AddExternalIdentity(string providerName, string providerUserId, Guid userId)
        {
            _idLookup[string.Format("{0}:{1}", providerName, providerUserId)] = userId;
        }
    }
}