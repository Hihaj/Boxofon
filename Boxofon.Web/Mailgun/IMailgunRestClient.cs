namespace Boxofon.Web.Mailgun
{
    public interface IMailgunRestClient
    {
        void SendMessage(string to, string from, string subject, string htmlBody);
    }
}