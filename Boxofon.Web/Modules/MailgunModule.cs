using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Indexes;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Model;
using Boxofon.Web.Security;
using Boxofon.Web.Twilio;
using NLog;
using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public class MailgunModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEmailAddressIndex _emailAddressIndex;
        private readonly IUserRepository _userRepository;
        private readonly ITwilioClientFactory _twilioClientFactory;

        public MailgunModule(
            IEmailAddressIndex emailAddressIndex,
            IUserRepository userRepository,
            ITwilioClientFactory twilioClientFactory) : base("/mailgun")
        {
            if (emailAddressIndex == null)
            {
                throw new ArgumentNullException("emailAddressIndex");
            }
            _emailAddressIndex = emailAddressIndex;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            if (twilioClientFactory == null)
            {
                throw new ArgumentNullException("twilioClientFactory");
            }
            _twilioClientFactory = twilioClientFactory;

            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            this.RequiresValidMailgunSignature();

            Post["/sms"] = parameters =>
            {
                var sendSmsCommand = new MailgunRequest(Request).ToSendSmsCommand();
                var userId = _emailAddressIndex.GetBoxofonUserId(sendSmsCommand.SenderEmail);
                User user = userId.HasValue ? _userRepository.GetById(userId.Value) : null;
                if (user == null)
                {
                    Logger.Error("Received an e-mail from a sender that does not match any existing user. Sender: '{0}'", sendSmsCommand.SenderEmail);
                    return HttpStatusCode.NotAcceptable; // Prevent Mailgun from retrying but signal that there was an error.
                }
                if (user.TwilioPhoneNumber != sendSmsCommand.SenderBoxofonNumber)
                {
                    Logger.Error("Received a SendSms mail command where the user does not own the specified Boxofon number. Sender e-mail: '{0}' Sender Boxofon number: '{1}'", sendSmsCommand.SenderEmail, sendSmsCommand.SenderBoxofonNumber);
                    return HttpStatusCode.NotAcceptable; // Prevent Mailgun from retrying but signal that there was an error.
                }
                var twilio = _twilioClientFactory.GetClientForUser(user);
                twilio.SendSmsMessage(
                    from: sendSmsCommand.SenderBoxofonNumber,
                    to: sendSmsCommand.RecipientPhoneNumber,
                    body: sendSmsCommand.Text);
                return HttpStatusCode.OK;
            };
        }
    }
}