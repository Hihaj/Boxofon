using System;
using Boxofon.Web.MailCommands;
using TinyMessenger;

namespace Boxofon.Web.Helpers
{
    public static class MailCommandExtensions
    {
        public static ITinyMessage ToTinyMessage(this IMailCommand mailCommand)
        {
            mailCommand.ThrowIfNull("mailCommand");
            return CommandToMessage((dynamic)mailCommand);
        }

        private static ITinyMessage CommandToMessage(SendSms command)
        {
            return new Messages.MailCommand<SendSms>(command);
        }
    }
}