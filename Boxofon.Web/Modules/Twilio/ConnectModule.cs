using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Indexes;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Messages;
using Boxofon.Web.Model;
using Boxofon.Web.Security;
using NLog;
using Nancy;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules.Twilio
{
    public class ConnectModule : NancyModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUserRepository _userRepository;
        private readonly ITwilioAccountIndex _twilioAccountIndex;
        private readonly ITinyMessengerHub _hub;

        public ConnectModule(
            IUserRepository userRepository, 
            ITwilioAccountIndex twilioAccountIndex, 
            ITinyMessengerHub hub)
            : base("/twilio/connect")
        {
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
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;
            
            this.RequiresHttps();
            this.RequiresWebhookAuthKey();
            //this.RequiresValidTwilioSignature();

            Post["/deauthorize"] = parameters =>
            {
                var twilioUserAccountSid = Request.Form["AccountSid"];
                var boxofonConnectAppSid = Request.Form["ConnectAppSid"];

                if (boxofonConnectAppSid != WebConfigurationManager.AppSettings["twilio:ConnectAppSid"])
                {
                    Logger.Info("Received a Twilio Connect deauthorization request with the wrong ConnectAppSid ('{0}').", boxofonConnectAppSid);
                    return HttpStatusCode.BadRequest;
                }

                if (string.IsNullOrEmpty(twilioUserAccountSid))
                {
                    Logger.Info("Received a Twilio Connect deauthorization request without AccountSid.");
                    return HttpStatusCode.BadRequest;
                }

                var userId = _twilioAccountIndex.GetBoxofonUserId((string)twilioUserAccountSid);
                User user = userId.HasValue ? _userRepository.GetById(userId.Value) : null;
                if (user == null)
                {
                    Logger.Info("Received a Twilio Connect deauthorization request for a user that does not exist (AccountSid = '{0}').", twilioUserAccountSid);
                    return HttpStatusCode.OK;
                }

                Logger.Debug("Received a Twilio Connect deauthorization request for user '{0}'.", user.Id);
                user.TwilioAccountSid = null;
                _userRepository.Save(user);
                _hub.PublishAsync(new UnlinkedTwilioAccountFromUser
                {
                    TwilioAccountSid = (string)twilioUserAccountSid,
                    UserId = userId.Value
                });
                return HttpStatusCode.OK;
            };
        }
    }
}