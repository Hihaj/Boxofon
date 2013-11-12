using Nancy;

namespace Boxofon.Web.Mailgun
{
    public class MailgunRequest
    {
        public string Sender { get; private set; }
        public string Recipient { get; private set; }
        public string StrippedText { get; private set; }
        public DkimValidationResult? DkimValidationResult { get; private set; }
        public SpfValidationResult SpfValidationResult { get; private set; }

        public MailgunRequest(Request request)
        {
            Sender = request.Form["sender"];
            Recipient = request.Form["recipient"];
            StrippedText = request.Form["stripped-text"];
        }
    }

    public enum DkimValidationResult
    {
        Pass,
        Fail
    }

    public enum SpfValidationResult
    {
        Pass,
        Neutral,
        Fail,
        SoftFail
    }
}