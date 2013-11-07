using System;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class AddedExternalIdentityToUser : ITinyMessage
    {
        public object Sender { get { return null; } }

        public string ProviderName { get; set; }
        public string ProviderUserId { get; set; }
        public Guid UserId { get; set; }
    }
}