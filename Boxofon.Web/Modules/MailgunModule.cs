using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Indexes;
using Boxofon.Web.MailCommands;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Model;
using Boxofon.Web.Security;
using Boxofon.Web.Twilio;
using NLog;
using Nancy;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules
{
    public class MailgunModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IMailCommandFactory _mailCommandFactory;
        private readonly ITinyMessengerHub _hub;

        public MailgunModule(
            IMailCommandFactory mailCommandFactory,
            ITinyMessengerHub hub) : base("/mailgun")
        {
            mailCommandFactory.ThrowIfNull("mailCommandFactory");
            hub.ThrowIfNull("hub");

            _mailCommandFactory = mailCommandFactory;
            _hub = hub;

            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            this.RequiresValidMailgunSignature();

            Post["/inbox"] = parameters =>
            {
                var request = new MailgunRequest(Request);
                try
                {
                    var command = _mailCommandFactory.Create(request);
                    _hub.Publish(command.ToTinyMessage());
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex.Message, ex);
                    return HttpStatusCode.NotAcceptable;
                }
            };
        }
    }
}