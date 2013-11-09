using System;
using System.Collections.Generic;
using Boxofon.Web.Messages;
using TinyMessenger;

namespace Boxofon.Web.Twilio
{
    public class InMemoryTwilioAccountLookup : TwilioAccountLookupBase
    {
        private readonly Dictionary<string, Guid>  _idLookup = new Dictionary<string, Guid>();

        public InMemoryTwilioAccountLookup(ITinyMessengerHub hub) : base(hub)
        {
        }

        public override Guid? GetBoxofonUserId(string twilioAccountSid)
        {
            Guid userId;
            if (_idLookup.TryGetValue(twilioAccountSid, out userId))
            {
                return userId;
            }
            return null;
        }

        protected override void AddTwilioAccount(string twilioAccountSid, Guid userId)
        {
            if (!string.IsNullOrEmpty(twilioAccountSid))
            {
                _idLookup[twilioAccountSid] = userId;
            }
        }

        protected override void RemoveTwilioAccount(string twilioAccountSid, Guid userId)
        {
            if (!string.IsNullOrEmpty(twilioAccountSid))
            {
                _idLookup.Remove(twilioAccountSid);
            }
        }
    }
}