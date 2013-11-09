using System;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Twilio
{
    public abstract class TwilioAccountLookupBase : ITwilioAccountLookup
    {
        protected readonly ITinyMessengerHub Hub;

        protected TwilioAccountLookupBase(ITinyMessengerHub hub)
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            Hub = hub;

            Hub.Subscribe<LinkedTwilioAccountToUser>(msg => AddTwilioAccount(msg.TwilioAccountSid, msg.UserId));
            Hub.Subscribe<UnlinkedTwilioAccountFromUser>(msg => RemoveTwilioAccount(msg.TwilioAccountSid, msg.UserId));
        }

        public abstract Guid? GetBoxofonUserId(string twilioAccountSid);
        protected abstract void AddTwilioAccount(string twilioAccountSid, Guid userId);
        protected abstract void RemoveTwilioAccount(string twilioAccountSid, Guid userId);
    }
}