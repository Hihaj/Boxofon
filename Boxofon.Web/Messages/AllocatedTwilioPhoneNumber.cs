using System;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class AllocatedTwilioPhoneNumber : ITinyMessage
    {
        public object Sender { get { return null; } }

        public string PhoneNumber { get; set; }
        public string FriendlyName { get; set; }
        public string WebhookAuthKey { get; set; }
        public string TwilioAccountSid { get; set; }
        public Guid UserId { get; set; }
    }
}