﻿using System;
using System.Linq;
using Boxofon.Web.Helpers;
using Boxofon.Web.Model;
using Boxofon.Web.Services;
using Nancy;
using Nancy.Helpers;
using Nancy.Security;
using TinyMessenger;

namespace Boxofon.Web.Modules.Account
{
    public class PrivateNumbersModule : WebsiteBaseModule
    {
        private readonly ITinyMessengerHub _hub;
        private readonly IPhoneNumberVerificationService _phoneNumberVerificationService;
        private readonly IUserRepository _userRepository;

        public PrivateNumbersModule(
            ITinyMessengerHub hub, 
            IPhoneNumberVerificationService phoneNumberVerificationService,
            IUserRepository userRepository)
            : base("/account/numbers/private")
        {
            if (hub == null)
            {
                throw new ArgumentNullException("hub");
            }
            _hub = hub;
            if (phoneNumberVerificationService == null)
            {
                throw new ArgumentNullException("phoneNumberVerificationService");
            }
            _phoneNumberVerificationService = phoneNumberVerificationService;
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;

            this.RequiresAuthentication();

            Get["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                if (!string.IsNullOrEmpty(user.PrivatePhoneNumber))
                {
                    Request.AddAlertMessage("error", "Det finns redan ett privat nummer angivet. Ta bort det först om du vill använda ett annat.");
                    return Response.AsRedirect("/account");
                }
                return View["Index.cshtml"];
            };

            Post["/"] = parameters =>
            {
                var user = this.GetCurrentUser();
                if (!string.IsNullOrEmpty(user.PrivatePhoneNumber))
                {
                    Request.AddAlertMessage("error", "Det finns redan ett privat nummer angivet. Ta bort det först om du vill använda ett annat.");
                    return Response.AsRedirect("/account");
                }
                var phoneNumber = (string)Request.Form.phoneNumber;
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    Request.AddAlertMessage("error", "Du måste ange ett mobilnummer.");
                    return View["Index.cshtml"];
                }
                if (!phoneNumber.IsPossiblyValidPhoneNumber())
                {
                    Request.AddAlertMessage("error", "Mobilnumret ser ut att vara ogiltigt. Kontrollera att du skrivit rätt.");
                    ViewBag.PhoneNumber = phoneNumber;
                    return View["Index.cshtml"];
                }
                phoneNumber = phoneNumber.ToE164();
                _phoneNumberVerificationService.BeginVerification(user, phoneNumber);
                return Response.AsRedirect(string.Format("/account/numbers/private/{0}/verification", phoneNumber.Replace("+", "00")));
            };

            Get["/{phoneNumber}/verification"] = parameters =>
            {
                return View["Verification.cshtml"];
            };

            Post["/{phoneNumber}/verification"] = parameters =>
            {
                var user = this.GetCurrentUser();
                var phoneNumber = ((string)parameters.phoneNumber).ToE164();
                var code = (string)Request.Form.code;
                var verificationSucceeded = _phoneNumberVerificationService.TryCompleteVerification(user, phoneNumber, code);
                if (!verificationSucceeded)
                {
                    Request.AddAlertMessage("error", "Felaktig verifieringskod.");
                    return View["Verification.cshtml"];
                }
                user.PrivatePhoneNumber = phoneNumber;
                _userRepository.Save(user);
                Request.AddAlertMessage("success", "Ditt privata mobilnummer är nu bekräftat.");
                return Response.AsRedirect("/account");
            };
        }
    }
}