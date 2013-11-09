namespace Boxofon.Web.Mailgun
{
    public interface IMailgunClient
    {
        void SendMessage(string to, string from, string subject, string htmlBody);
        void SendNoReplyMessage(string to, string subject, string htmlBody);
    }
}