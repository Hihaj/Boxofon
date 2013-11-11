using System;

namespace Boxofon.Web.Model
{
    public class TwilioPhoneNumber
    {
        public string PhoneNumber { get; set; }
        public string FriendlyName { get; set; }
        public string WebhookAuthKey { get; set; }
        public string TwilioAccountSid { get; set; }
        public Guid UserId { get; set; }
    }
}