using System;
using System.Text.RegularExpressions;

namespace Boxofon.Web.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex PhoneNumberRegex = new Regex(@"^(\+)?\d+$", RegexOptions.Compiled);

        public static bool IsPossiblyValidPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            phoneNumber = phoneNumber
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);
            return PhoneNumberRegex.IsMatch(phoneNumber);
        }

        public static string ToE164(this string phoneNumber)
        {
            if (!IsPossiblyValidPhoneNumber(phoneNumber))
            {
                throw new FormatException("The input is not a valid phone number.");
            }
            phoneNumber = phoneNumber
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);
            if (phoneNumber.StartsWith("+"))
            {
                return phoneNumber;
            }
            if (phoneNumber.StartsWith("00"))
            {
                return "+" + phoneNumber.Remove(0, 2);
            }
            if (phoneNumber.StartsWith("0"))
            {
                return "+46" + phoneNumber.Remove(0, 1);
            }
            return "+46" + phoneNumber;
        }
    }
}