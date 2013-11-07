using System;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class LinkedTwilioAccountToUser : ITinyMessage
    {
        public object Sender { get { return null; } }

        public string TwilioAccountSid { get; set; }
        public Guid UserId { get; set; }
    }
}