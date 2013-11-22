using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Boxofon.Web.Helpers
{
    public static class StringExtensions
    {
        private const string PhoneNumberPattern = @"\+?(\d+-)?\d+";
        private static readonly Regex PhoneNumberRegex = new Regex("^" + PhoneNumberPattern + "$", RegexOptions.Compiled);
        private static readonly Regex PhoneNumbersRegex = new Regex(@"(\s|,|;|^|\G)+(?<phoneNumber>" + PhoneNumberPattern + @")(\s|,|;|$)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static readonly Regex EmailSubjectAbbrevationRegex = new Regex(@"^((re|fw|sv|vs|antw|doorst|vl|tr|aw|wg|r|rif|i|fs|rv|res|enc|odp|pd|vb):\s*)+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

        public static string[] GetAllPhoneNumbers(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new string[0];
            }
            return (from Match match in PhoneNumbersRegex.Matches(text)
                    where match.Groups["phoneNumber"].Success
                    select match.Groups["phoneNumber"].Value.ToE164()).Distinct().ToArray();
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ZBase32Encode(this string unencodedValue)
        {
            var bytes = Encoding.UTF8.GetBytes(unencodedValue);
            return new ZBase32().Encode(bytes);
        }

        public static string ZBase32Decode(this string encodedValue)
        {
            var bytes = new ZBase32().Decode(encodedValue);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string HmacSha256HexDigestEncode(this string value, string key)
        {
            var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static string HmacSha1Base64Encode(this string value, string key)
        {
            var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(hash);
        }

        public static string Truncate(this string value, int maxLength, string suffixWhenTruncated = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }
            return value.Substring(0, maxLength) + suffixWhenTruncated;
        }

        public static string RemoveCommonEmailSubjectAbbrevations(this string subject)
        {
            return string.IsNullOrEmpty(subject) ? subject : EmailSubjectAbbrevationRegex.Replace(subject, string.Empty);
        }
    }
}