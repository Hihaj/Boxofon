using System;
using Boxofon.Web.Membership;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class UserCreated : ITinyMessage
    {
        public object Sender { get { return null; } }

        public Guid UserId { get; set; }
        public ExternalIdentity ExternalIdentity { get; set; }
    }
}