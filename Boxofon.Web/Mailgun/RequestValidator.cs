using Boxofon.Web.Helpers;
using NLog;
using Nancy;

namespace Boxofon.Web.Mailgun
{
    public class RequestValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
 
        public bool IsValidRequest(NancyContext context, string apiKey)
        {
            var timestamp = (int)context.Request.Form.timestamp;
            var token = (string)context.Request.Form.token;
            var signature = (string)context.Request.Form.signature;
            var unencoded = string.Format("{0}{1}", timestamp, token);
            var encoded = unencoded.HmacSha256HexDigestEncode(apiKey);

            if (encoded != signature)
            {
                Logger.Info("Validation of Mailgun request failed. Timestamp: '{0}' Token: '{1}' Signature: '{2}' Computed: '{3}'", timestamp, token, signature, encoded);
                return false;
            }

            return true;
        }
    }
}