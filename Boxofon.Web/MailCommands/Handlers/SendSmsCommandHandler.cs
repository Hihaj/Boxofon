using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Model;
using Boxofon.Web.Twilio;
using NLog;
using TinyMessenger;

namespace Boxofon.Web.MailCommands.Handlers
{
    public class SendSmsCommandHandler : MailCommandHandlerBase<SendSms>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITwilioClientFactory _twilioClientFactory;
        private readonly IUserRepository _userRepository;

        public SendSmsCommandHandler(ITwilioClientFactory twilioClientFactory, IUserRepository userRepository)
        {
            twilioClientFactory.ThrowIfNull("twilioClientFactory");
            userRepository.ThrowIfNull("userRepository");
            _twilioClientFactory = twilioClientFactory;
            _userRepository = userRepository;
        }
        
        protected override void Handle(SendSms command)
        {
            var user = _userRepository.GetById(command.UserId);
            if (user == null)
            {
                Logger.Error("Cannot find user '{0}'.", command.UserId);
                return;
            }
            if (command.BoxofonNumber != user.TwilioPhoneNumber)
            {
                Logger.Error("User does not own the Boxofon number '{0}'.", command.BoxofonNumber);
                return;
            }
            foreach (var recipient in command.RecipientPhoneNumbers)
            {
                var twilio = _twilioClientFactory.GetClientForUser(user);
                try
                {
                    twilio.SendSmsMessage(command.BoxofonNumber, recipient, command.Text);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(string.Format("Error sending SMS from '{0}' to '{1}'.", command.BoxofonNumber, recipient), ex);
                }
            }
        }
    }
}