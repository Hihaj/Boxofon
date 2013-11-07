using System;
using System.Collections.Generic;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Twilio
{
    public class InMemoryTwilioAccountService : ITwilioAccountService
    {
        private readonly Dictionary<string, Guid>  _idLookup = new Dictionary<string, Guid>();
        private readonly ITinyMessengerHub _hub;

        public InMemoryTwilioAccountService(ITinyMessengerHub hub)
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;

            _hub.Subscribe<LinkedTwilioAccountToUser>(msg => AddTwilioAccount(msg.TwilioAccountSid, msg.UserId));
            _hub.Subscribe<UnlinkedTwilioAccountFromUser>(msg => RemoveTwilioAccount(msg.TwilioAccountSid, msg.UserId));
        }

        public Guid? GetBoxofonUserId(string twilioAccountSid)
        {
            Guid userId;
            if (_idLookup.TryGetValue(twilioAccountSid, out userId))
            {
                return userId;
            }
            return null;
        }

        private void AddTwilioAccount(string twilioAccountSid, Guid userId)
        {
            if (!string.IsNullOrEmpty(twilioAccountSid))
            {
                _idLookup[twilioAccountSid] = userId;
            }
        }

        private void RemoveTwilioAccount(string twilioAccountSid, Guid userId)
        {
            if (!string.IsNullOrEmpty(twilioAccountSid))
            {
                _idLookup.Remove(twilioAccountSid);
            }
        }
    }
}