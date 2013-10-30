using Nancy;
using Nancy.Security;

namespace Boxofon.Web.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            this.RequiresHttps(redirect: true, httpsPort: 44300);

            Get["/"] = parameters => View["Default.cshtml"];
        }
    }
}