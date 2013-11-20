using Boxofon.Web.Mailgun;
using Boxofon.Web.Model;

namespace Boxofon.Web.MailCommands
{
    public interface IMailCommandFactory
    {
        IMailCommand Create(MailgunRequest request);
    }
}