using Boxofon.Web.Security;
using NLog;
using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public class MailgunModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MailgunModule() : base("/mailgun")
        {
            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            this.RequiresValidMailgunSignature();

            Post["/sms"] = parameters =>
            {
                var sender = (string)Request.Form["sender"];
                var recipient = (string)Request.Form["recipient"];
                var text = (string)Request.Form["stripped-text"];
                Logger.Debug("Received mail-to-sms request. Sender: '{0}' Recipient: '{1}' Text: '{2}'", sender, recipient, text);
                return HttpStatusCode.OK;
            };
        }
    }
}