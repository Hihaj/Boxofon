using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Boxofon.Web.Filters
{
    public class RequireWebhookAuthKeyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Request.QueryString["authKey"] == WebConfigurationManager.AppSettings["WebhookAuthKey"];
        }
    }
}