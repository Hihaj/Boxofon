using System;
using System.Linq;
using System.Web.Configuration;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Security;
using NLog;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.Session;
using Nancy.TinyIoc;
using TinyMessenger;

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
            Nancy.Security.Csrf.Enable(pipelines);
            StaticConfiguration.CaseSensitive = true;

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
            
            pipelines.AfterRequest.AddItemToEndOfPipeline(context =>
            {
                if (context.Items.ContainsKey(SecurityHooks.ForceHttpStatusCodeKey))
                {
                    context.Response = new Response { StatusCode = (HttpStatusCode)context.Items[SecurityHooks.ForceHttpStatusCodeKey] };
                }
            });

            // Initialize components
            foreach (var component in container.ResolveAll<IRequireInitialization>())
            {
                component.Initialize();
            }

            // Register subscriptions
            var hub = container.Resolve<ITinyMessengerHub>();
            foreach (var subscriber in container.ResolveAll<ISubscriber>())
            {
                subscriber.RegisterSubscriptions(hub);
            }
        }
    }
}