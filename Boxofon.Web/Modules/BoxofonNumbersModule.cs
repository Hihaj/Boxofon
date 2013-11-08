using System;
using System.Linq;
using Boxofon.Web.Helpers;
using Boxofon.Web.Twilio;
using Boxofon.Web.ViewModels;
using Nancy;
using Nancy.Security;
using Twilio;

namespace Boxofon.Web.Modules
{
    public class BoxofonNumbersModule : WebsiteBaseModule
    {
        private readonly ITwilioClientFactory _twilioClientFactory;

        public BoxofonNumbersModule(ITwilioClientFactory twilioClientFactory) : base("/account/numbers/boxofon")
        {
            if (twilioClientFactory == null)
            {
                throw new ArgumentNullException("twilioClientFactory");
            }
            _twilioClientFactory = twilioClientFactory;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                return View["Index.cshtml"];
            };

            Get["/available"] = parameters =>
            {
                var twilio = _twilioClientFactory.GetUserClient(this.GetCurrentUser());
                var availableNumbers = twilio.ListAvailableLocalPhoneNumbers("SE", new AvailablePhoneNumberListRequest());
                var viewModel = new ViewModels.AvailableBoxofonNumbers
                {
                    Numbers = availableNumbers.AvailablePhoneNumbers.Select(number => new BoxofonNumber
                    {
                        FriendlyName = number.FriendlyName,
                        PhoneNumber = number.PhoneNumber
                    }).ToArray()
                };
                return Response.AsJson(viewModel);
            };
        }
    }
}