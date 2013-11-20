using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Model;

namespace Boxofon.Web.MailCommands
{
    public class SendSms : IMailCommand
    {
        public Guid UserId { get; set; }
        public string BoxofonNumber { get; set; }
        public string[] RecipientPhoneNumbers { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("Send SMS from '{0}' to '{1}'", BoxofonNumber, string.Join(",", RecipientPhoneNumbers));
        }
    }
}