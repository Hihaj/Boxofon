﻿using Nancy;
using Nancy.Extensions;

namespace Boxofon.Web.Security
{
    public static class ModuleSecurity
    {
        public static void RequiresWebhookAuthKey(this INancyModule module)
        {
            module.AddBeforeHookOrExecute(SecurityHooks.RequiresWebhookAuthKey(), "Requires webhook auth key");
        }
    }
}