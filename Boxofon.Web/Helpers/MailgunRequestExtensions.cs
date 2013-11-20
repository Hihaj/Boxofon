using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Boxofon.Web.Mailgun;

namespace Boxofon.Web.Helpers
{
    public static class MailgunRequestExtensions
    {
        public static string BoxofonNumber(this MailgunRequest request)
        {
            var from = new MailAddress(request.From);
            if (from.User.IsPossiblyValidPhoneNumber())
            {
                return from.User.ToE164();
            }
            return null;
        }
    }
}