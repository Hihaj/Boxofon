﻿using System.Net;
using System.Web.Configuration;
using ServiceStack.Text;

namespace Boxofon.Web.Mailgun
{
    public class MailgunClient : IMailgunClient
    {
        private readonly string _apiKey;
        private readonly string _domain;

        public MailgunClient()
        {
            // TODO Inject settings
            _apiKey = WebConfigurationManager.AppSettings["mailgun:ApiKey"];
            _domain = WebConfigurationManager.AppSettings["mailgun:Domain"];
        }

        public void SendMessage(string to, string from, string subject, string htmlBody)
        {
            "https://api.mailgun.net/v2/{0}/messages"
                .Fmt(_domain)
                .PostToUrl(new
                {
                    from = from,
                    to = to,
                    subject = subject,
                    html = htmlBody
                },
                requestFilter: webRequest => { webRequest.Credentials = new NetworkCredential("api", _apiKey); });
        }

        public void SendNoReplyMessage(string to, string subject, string htmlBody)
        {
            SendMessage(to, WebConfigurationManager.AppSettings["boxofon:NoreplyEmail"], subject, htmlBody);
        }
    }
}