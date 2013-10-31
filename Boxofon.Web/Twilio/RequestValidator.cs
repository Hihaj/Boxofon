using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NLog;
using Nancy;

namespace Boxofon.Web.Twilio
{
    public class RequestValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        public bool IsValidRequest(NancyContext context, string authToken)
        {
            return IsValidRequest(context, authToken, null);
        }

        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        public bool IsValidRequest(NancyContext context, string authToken, string urlOverride)
        {
            if (context.Request.Url.HostName == "localhost")
            {
                return true;
            }

            // validate request
            // http://www.twilio.com/docs/security-reliability/security
            // Take the full URL of the request, from the protocol (http...) through the end of the query string (everything after the ?)
            var value = new StringBuilder();
            var fullUrl = string.IsNullOrEmpty(urlOverride) ? ((Uri)context.Request.Url).AbsoluteUri : urlOverride;
            value.Append(fullUrl);

            // If the request is a POST, take all of the POST parameters and sort them alphabetically.
            if (context.Request.Method == "POST")
            {
                // Iterate through that sorted list of POST parameters, and append the variable name and value (with no delimiters) to the end of the URL string
                var sortedKeys = ((DynamicDictionary)context.Request.Form).Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
                foreach (var key in sortedKeys)
                {
                    value.Append(key);
                    value.Append((string)context.Request.Form[key]);
                }
            }
            // Sign the resulting value with HMAC-SHA1 using your AuthToken as the key (remember, your AuthToken's case matters!).
            var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(authToken));
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()));

            // Base64 encode the hash
            var encoded = Convert.ToBase64String(hash);

            // Compare your hash to ours, submitted in the X-Twilio-Signature header. If they match, then you're good to go.
            var signature = context.Request.Headers["X-Twilio-Signature"].FirstOrDefault();

            var requestIsValid = !string.IsNullOrEmpty(signature) && signature == encoded;
            if (!requestIsValid)
            {
                Logger.Info("Validation of incoming Twilio request failed ({0}).", 
                    string.IsNullOrEmpty(signature) ? "signature missing" : "signature mismatch");
            }
            return requestIsValid;
        } 
    }
}