using System;
using Boxofon.Web.Helpers;
using Boxofon.Web.Mailgun;

namespace Boxofon.Web.MailCommands
{
    public class InvalidMailCommandException : Exception
    {
        public InvalidMailCommandException(MailgunRequest request) : base(string.Format(
            "Invalid mail command. From: '{0}' To: '{1}' Subject (first 50 chars): '{2}' Body (first 50 chars): '{3}'", 
            request.From, 
            request.To, 
            request.Subject.Truncate(50),
            request.StrippedText.Truncate(50)))
        {
        }

        public InvalidMailCommandException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedMailCommandException : Exception
    {
        public UnauthorizedMailCommandException(string fromEmail, string boxofonNumber) : base(string.Format(
            "Unauthorized mail command. From: '{0}' Boxofon number: '{1}'", 
            fromEmail, 
            boxofonNumber))
        {
        }
    }

    public class UnknownMailCommandSenderException : Exception
    {
        public UnknownMailCommandSenderException(string fromEmail) : base(string.Format(
            "Unknown mail command sender: '{0}'", fromEmail))
        {
        }
    }
}