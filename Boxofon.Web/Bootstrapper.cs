using System;
using System.Linq;
using System.Web.Configuration;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Security;
using NLog;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
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

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            // Cryptography components
            container.Register<IKeyGenerator>(new PassphraseKeyGenerator(WebConfigurationManager.AppSettings["boxofon:EncryptionKeyPassphrase"], new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            container.Register<IEncryptionProvider>(new RijndaelEncryptionProvider(container.Resolve<IKeyGenerator>()));
            container.Register<IHmacProvider>(new DefaultHmacProvider(container.Resolve<IKeyGenerator>()));
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

            // Forms authentication
            var formsAuthConfiguration = new Nancy.Authentication.Forms.FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/account/signin",
                RequiresSSL = true,
                UserMapper = container.Resolve<IUserMapper>(),
                CryptographyConfiguration = new CryptographyConfiguration(container.Resolve<IEncryptionProvider>(), container.Resolve<IHmacProvider>())
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