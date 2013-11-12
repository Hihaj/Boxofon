using System;
using System.Text.RegularExpressions;
using Boxofon.Web.Mailgun;

namespace Boxofon.Web.Helpers
{
    public static class MailgunRequestExtensions
    {
        private static readonly Regex SendSmsLocalPartRegex = new Regex(@"(?<to>\+?[0-9\-]+).(?<from>\+?[0-9\-]+)", RegexOptions.Compiled);

        public static MailCommands.SendSms ToSendSmsCommand(this MailgunRequest request)
        {
            var sendSms = new MailCommands.SendSms
            {
                SenderEmail = request.Sender,
                Text = request.StrippedText
            };
            var localPart = GetLocalPart(request.Recipient);
            var match = SendSmsLocalPartRegex.Match(localPart);
            if (!match.Success)
            {
                throw new FormatException("Recipient e-mail address does not match the required format for mail-to-SMS requests.");
            }
            sendSms.SenderBoxofonNumber = match.Groups["from"].Value.ToE164();
            sendSms.RecipientPhoneNumber = match.Groups["to"].Value.ToE164();
            return sendSms;
        }

        private static string GetLocalPart(string email)
        {
            var lastIndexOfAtSign = email.LastIndexOf("@", StringComparison.InvariantCultureIgnoreCase);
            if (lastIndexOfAtSign < 0)
            {
                throw new FormatException("Could not extract local part from e-mail address.");
            }
            return email.Substring(0, lastIndexOfAtSign);
        }
    }
}