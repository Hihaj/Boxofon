using System;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public abstract class TwilioAccountLookupBase : ITwilioAccountLookup, ISubscriber
    {
        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            hub.Subscribe<LinkedTwilioAccountToUser>(msg => AddTwilioAccount(msg.TwilioAccountSid, msg.UserId));
            hub.Subscribe<UnlinkedTwilioAccountFromUser>(msg => RemoveTwilioAccount(msg.TwilioAccountSid, msg.UserId));
        }

        public abstract Guid? GetBoxofonUserId(string twilioAccountSid);
        protected abstract void AddTwilioAccount(string twilioAccountSid, Guid userId);
        protected abstract void RemoveTwilioAccount(string twilioAccountSid, Guid userId);
    }
}