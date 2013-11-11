using System;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class RemovedExternalIdentityFromUser : ITinyMessage
    {
        public object Sender { get { return null; } }

        public string ProviderName { get; set; }
        public string ProviderUserId { get; set; }
        public Guid UserId { get; set; }
    }
}