﻿using System.Web.Configuration;
using NLog;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.Session;
using Nancy.TinyIoc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;

namespace Boxofon.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = WebConfigurationManager.AppSettings["nancy:DiagnosticsPassword"] }; }
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);

            pipelines.OnError += (ctx, ex) =>
            {
                Logger.ErrorException(ex.Message, ex);
                return null;
            };

            var formsAuthConfiguration = new Nancy.Authentication.Forms.FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/account/signin",
                RequiresSSL = true,
                UserMapper = container.Resolve<IUserMapper>()
                // TODO Configure cryptography!
            };
            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }
}