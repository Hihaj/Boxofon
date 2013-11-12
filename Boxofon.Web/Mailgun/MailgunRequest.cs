using Nancy;

namespace Boxofon.Web.Mailgun
{
    public class MailgunRequest
    {
        public string Sender { get; private set; }
        public string Recipient { get; private set; }
        public string StrippedText { get; private set; }

        public MailgunRequest(Request request)
        {
            Sender = request.Form["sender"];
            Recipient = request.Form["recipient"];
            StrippedText = request.Form["stripped-text"];
        }
    }
}