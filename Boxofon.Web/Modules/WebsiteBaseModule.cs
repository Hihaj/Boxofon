using Boxofon.Web.Helpers;
using Boxofon.Web.Infrastructure;
using NLog;
using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public abstract class WebsiteBaseModule : NancyModule
    {
        protected WebsiteBaseModule()
        {
            this.RequiresHttps();

            After.AddItemToEndOfPipeline(AlertsToViewBag);
            After.AddItemToEndOfPipeline(RemoveAlerts);
        }

        protected WebsiteBaseModule(string modulePath) : base(modulePath)
        {
            this.RequiresHttps();

            After.AddItemToEndOfPipeline(AlertsToViewBag);
            After.AddItemToEndOfPipeline(RemoveAlerts);
        }

        internal static void AlertsToViewBag(NancyContext context)
        {
            var alerts = context.Request.Session.GetSessionValue<AlertMessageStore>(AlertMessageStore.AlertMessageKey);
            context.ViewBag.Alerts = alerts;
        }

        internal static void RemoveAlerts(NancyContext context)
        {
            if (context.Response.StatusCode != HttpStatusCode.Unauthorized &&
                context.Response.StatusCode != HttpStatusCode.SeeOther &&
                context.Response.StatusCode != HttpStatusCode.Found)
            {
                context.Request.Session.Delete(AlertMessageStore.AlertMessageKey);
            }
        } 
    }
}