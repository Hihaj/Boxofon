using System;
using System.Web.Configuration;
using Boxofon.Web.Twilio;
using NLog;
using Nancy;
using Nancy.Responses;

namespace Boxofon.Web.Security
{
    public static class SecurityHooks
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly RequestValidator TwilioRequestValidator = new RequestValidator();

        public const string ForceHttpStatusCodeKey = "ForceHttpStatusCode";

        public static Func<NancyContext, Response> RequiresWebhookAuthKey()
        {
            return UnauthorizedIfNot(ctx =>
            {
                var authKey = ctx.Request.Query.authKey;
                var isValidAuthKey = !string.IsNullOrEmpty(authKey) && authKey == WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"];
                if (!isValidAuthKey)
                {
                    
                    Logger.Info("Received an unauthorized webhook request ({0}).",
                        string.IsNullOrEmpty(authKey) ? "authKey missing" : "authKey mismatch");
                }
                return isValidAuthKey;
            }, forceStatusCodeResult: true);
        }

        public static Func<NancyContext, Response> RequiresValidTwilioSignature()
        {
            return UnauthorizedIfNot(ctx =>
            {
                return TwilioRequestValidator.IsValidRequest(ctx, WebConfigurationManager.AppSettings["twilio:AuthToken"]);
            }, forceStatusCodeResult: true);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Unauthorized response if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> UnauthorizedIfNot(Func<NancyContext, bool> test, bool forceStatusCodeResult = false)
        {
            return HttpStatusCodeIfNot(HttpStatusCode.Unauthorized, test, forceStatusCodeResult);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Forbidden response if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test, bool forceStatusCodeResult = false)
        {
            return HttpStatusCodeIfNot(HttpStatusCode.Forbidden, test, forceStatusCodeResult);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="statusCode">HttpStatusCode to use for the response</param>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns a response with a specific HttpStatusCode if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test, bool forceStatusCodeResult = false)
        {
            return (ctx) =>
            {
                Response response = null;
                if (!test(ctx))
                {
                    response = new Response { StatusCode = statusCode };
                    if (forceStatusCodeResult)
                    {
                        ctx.Items[ForceHttpStatusCodeKey] = statusCode;
                    }
                }

                return response;
            };
        }

    }
}