using System;
using System.Web.Configuration;
using Boxofon.Web.Indexes;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Model;
using Boxofon.Web.Security;
using NLog;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Twilio.Mvc;

namespace Boxofon.Web.Modules.Twilio
{
    public class SmsModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IMailgunClient _mailgun;
        private readonly IUserRepository _userRepository;
        private readonly ITwilioAccountIndex _twilioAccountIndex;

        public SmsModule(
            IMailgunClient mailgun, 
            IUserRepository userRepository, 
            ITwilioAccountIndex twilioAccountIndex)
            : base("/twilio/sms")
        {
            if (mailgun == null)
            {
                throw new ArgumentNullException("mailgun");
            }
            _mailgun = mailgun;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            if (twilioAccountIndex == null)
            {
                throw new ArgumentNullException("twilioAccountIndex");
            }
            _twilioAccountIndex = twilioAccountIndex;
            
            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            //this.RequiresValidTwilioSignature();

            Post["/incoming"] = parameters =>
            {
                var request = this.Bind<SmsRequest>();
                var userId = _twilioAccountIndex.GetBoxofonUserId(request.AccountSid);
                User user = userId.HasValue ? _userRepository.GetById(userId.Value) : null;
                if (user == null)
                {
                    Logger.Error("Received an SMS for a user that does not exist. AccountSid: '{0}' SmsSid: '{1}'", request.AccountSid, request.SmsSid);
                    return HttpStatusCode.OK; // To prevent retries from Twilio - is this the best way?
                }
                if (string.IsNullOrEmpty(user.Email))
                {
                    Logger.Error("Received an SMS for a user that does not have an e-mail address. UserId: '{0}' SmsSid: '{1}'", user.Id, request.SmsSid);
                    return HttpStatusCode.OK;
                }

                try
                {
                    _mailgun.SendMessage(
                        to: user.Email,
                        from: string.Format("Boxofon <{0}@{1}>", request.To, WebConfigurationManager.AppSettings["mailgun:Domain"]),
                        subject: string.Format("SMS från {0}", request.From),
                        htmlBody: request.Body);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error sending mail.", ex);
                    return HttpStatusCode.InternalServerError;
                    // TODO Handle error (log?) - see https://github.com/ServiceStack/ServiceStack/wiki/Http-Utils#exception-handling
                }

                return HttpStatusCode.OK;
            };
        }
    }
}