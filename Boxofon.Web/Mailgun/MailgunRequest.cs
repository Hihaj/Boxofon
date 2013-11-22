using System;
using Boxofon.Web.Helpers;
using Nancy;

namespace Boxofon.Web.Mailgun
{
    public class MailgunRequest
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public string Subject { get; private set; }
        public string StrippedText { get; private set; }
        public DkimValidationResult? Dkim { get; private set; }
        public SpfValidationResult? Spf { get; private set; }
        
        public bool SentFromAuthenticatedServer
        {
            get
            {
                return Dkim.HasValue &&
                       Dkim.Value == DkimValidationResult.Pass &&
                       Spf.HasValue &&
                       Spf.Value == SpfValidationResult.Pass;
            }
        }

        public MailgunRequest(Request request)
        {
            From = request.Form["from"];
            To = request.Form["recipient"];
            Subject = request.Form["subject"];
            StrippedText = request.Form["stripped-text"];

            DkimValidationResult dkim;
            if (Enum.TryParse(request.Form["X-Mailgun-Dkim-Check-Result"], true, out dkim))
            {
                Dkim = dkim;
            }

            SpfValidationResult spf;
            if (Enum.TryParse(request.Form["X-Mailgun-Spf"], true, out spf))
            {
                Spf = spf;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "From: '{0}' To: '{1}' Subject: '{2}' Body: '{3}'",
                From,
                To,
                Subject.Truncate(50, " [truncated]"),
                StrippedText.Truncate(50, " [truncated]"));
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