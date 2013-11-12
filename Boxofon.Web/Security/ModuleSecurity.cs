using Nancy;
using Nancy.Extensions;

namespace Boxofon.Web.Security
{
    public static class ModuleSecurity
    {
        public static void RequiresWebhookAuthKey(this INancyModule module)
        {
            module.AddBeforeHookOrExecute(SecurityHooks.RequiresWebhookAuthKey(), "Requires webhook auth key.");
        }

        public static void RequiresValidTwilioSignature(this INancyModule module)
        {
            module.AddBeforeHookOrExecute(SecurityHooks.RequiresValidTwilioSignature(), "Requires valid Twilio signature.");
        }

        public static void RequiresValidMailgunSignature(this INancyModule module)
        {
            module.AddBeforeHookOrExecute(SecurityHooks.RequiresValidMailgunSignature(), "Requires valid Mailgun signature.");
        }
    }
}