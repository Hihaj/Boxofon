using Boxofon.Web.MailCommands;
using TinyMessenger;

namespace Boxofon.Web.Messages
{
    public class MailCommand<TMailCommand> : ITinyMessage where TMailCommand : class, IMailCommand
    {
        public object Sender
        {
            get { return null; }
        }

        public TMailCommand Command { get; set; }

        public MailCommand()
        {
        }

        public MailCommand(TMailCommand command)
        {
            Command = command;
        } 
    }
}