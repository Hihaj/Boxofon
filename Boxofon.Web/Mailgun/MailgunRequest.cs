using Nancy;

namespace Boxofon.Web.Mailgun
{
    public class MailgunRequest
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public string Subject { get; private set; }
        public string StrippedText { get; private set; }
        public DkimValidationResult? DkimValidationResult { get; private set; }
        public SpfValidationResult SpfValidationResult { get; private set; }

        public MailgunRequest(Request request)
        {
            From = request.Form["sender"];
            To = request.Form["recipient"];
            Subject = request.Form["subject"];
            StrippedText = request.Form["stripped-text"];

            // TODO Set DkimValidationResult
            // TODO Set SpfValidationResult
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