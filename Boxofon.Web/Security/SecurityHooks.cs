using System;
using System.Web.Configuration;
using Nancy;
using Nancy.Responses;

namespace Boxofon.Web.Security
{
    public static class SecurityHooks
    {
        public static Func<NancyContext, Response> RequiresWebhookAuthKey()
        {
            return UnauthorizedIfNot(ctx => ctx.Request.Query.authKey == WebConfigurationManager.AppSettings["boxofon:WebhookAuthKey"]);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Unauthorized response if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> UnauthorizedIfNot(Func<NancyContext, bool> test)
        {
            return HttpStatusCodeIfNot(HttpStatusCode.Unauthorized, test);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns an Forbidden response if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> ForbiddenIfNot(Func<NancyContext, bool> test)
        {
            return HttpStatusCodeIfNot(HttpStatusCode.Forbidden, test);
        }

        /// <summary>
        /// Creates a hook to be used in a pipeline before a route handler to ensure that
        /// the request satisfies a specific test.
        /// </summary>
        /// <param name="statusCode">HttpStatusCode to use for the response</param>
        /// <param name="test">Test that must return true for the request to continue</param>
        /// <returns>Hook that returns a response with a specific HttpStatusCode if the test fails, null otherwise</returns>
        private static Func<NancyContext, Response> HttpStatusCodeIfNot(HttpStatusCode statusCode, Func<NancyContext, bool> test)
        {
            return (ctx) =>
            {
                Response response = null;
                if (!test(ctx))
                {
                    response = new Response { StatusCode = statusCode };
                }

                return response;
            };
        }

    }
}