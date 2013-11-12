using System;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class AddedEmailToUser : ITinyMessage
    {
        public object Sender { get { return null; } }

        public string Email { get; set; }
        public Guid UserId { get; set; }
    }
}