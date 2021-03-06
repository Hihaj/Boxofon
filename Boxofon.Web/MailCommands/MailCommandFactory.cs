﻿using Boxofon.Web.Helpers;
using Boxofon.Web.Indexes;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Model;
using NLog;

namespace Boxofon.Web.MailCommands
{
    public class MailCommandFactory : IMailCommandFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEmailAddressIndex _emailAddressIndex;
        private readonly IUserRepository _userRepository;

        public MailCommandFactory(IEmailAddressIndex emailAddressIndex, IUserRepository userRepository)
        {
            emailAddressIndex.ThrowIfNull("emailAddressIndex");
            userRepository.ThrowIfNull("userRepository");

            _emailAddressIndex = emailAddressIndex;
            _userRepository = userRepository;
        }

        public IMailCommand Create(MailgunRequest request)
        {
            var userId = _emailAddressIndex.GetBoxofonUserId(request.From);
            User user = userId.HasValue ? _userRepository.GetById(userId.Value) : null;
            if (user == null)
            {
                throw new UnknownMailCommandSenderException(request.From);
            }

            var boxofonNumber = request.BoxofonNumber();
            if (string.IsNullOrEmpty(boxofonNumber))
            {
                throw new InvalidMailCommandException(request);
            }
            if (boxofonNumber != user.TwilioPhoneNumber)
            {
                throw new UnauthorizedMailCommandException(request.From, boxofonNumber);
            }

            var subject = request.Subject.RemoveCommonEmailSubjectAbbrevations().ToLowerInvariant();

            // TODO Parse into command in a more elegant way?
            if (subject.StartsWith("sms "))
            {
                var recipientPhoneNumbers = subject.GetAllPhoneNumbers();
                if (recipientPhoneNumbers.Length == 0)
                {
                    throw new InvalidMailCommandException("Invalid SMS mail command - recipient(s) missing.");
                }
                if (string.IsNullOrWhiteSpace(request.StrippedText))
                {
                    throw new InvalidMailCommandException("Invalid SMS mail command - text missing.");
                }
                return new SendSms
                {
                    UserId = user.Id,
                    BoxofonNumber = boxofonNumber,
                    RecipientPhoneNumbers = recipientPhoneNumbers,
                    Text = request.StrippedText.Trim()
                };
            }

            throw new InvalidMailCommandException(request);
        }
    }
}