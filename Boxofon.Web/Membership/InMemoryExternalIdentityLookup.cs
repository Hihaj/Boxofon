using System;
using System.Collections.Generic;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public class InMemoryExternalIdentityLookup : ExternalIdentityLookupBase
    {
        private readonly Dictionary<string, Guid>  _idLookup = new Dictionary<string, Guid>();

        public InMemoryExternalIdentityLookup(ITinyMessengerHub hub) : base(hub)
        {
        }

        public override Guid? GetBoxofonUserId(string providerName, string providerUserId)
        {
            Guid userId;
            if (_idLookup.TryGetValue(string.Format("{0}:{1}", providerName, providerUserId), out userId))
            {
                return userId;
            }
            return null;
        }

        protected override void AddExternalIdentity(string providerName, string providerUserId, Guid userId)
        {
            _idLookup[string.Format("{0}:{1}", providerName, providerUserId)] = userId;
        }
    }
}