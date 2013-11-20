using Boxofon.Web.Helpers;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Twilio;
using TinyMessenger;

namespace Boxofon.Web.MailCommands.Handlers
{
    public class SendSmsCommandHandler : MailCommandHandlerBase<SendSms>
    {
        private readonly ITwilioClientFactory _twilioClientFactory;

        public SendSmsCommandHandler(ITwilioClientFactory twilioClientFactory)
        {
            twilioClientFactory.ThrowIfNull("twilioClientFactory");
            _twilioClientFactory = twilioClientFactory;
        }
        
        protected override void Handle(SendSms command)
        {
            
        }
    }
}