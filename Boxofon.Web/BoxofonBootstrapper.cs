using System.Web.Configuration;
using Nancy;
using Nancy.Diagnostics;

namespace Boxofon.Web
{
    public class BoxofonBootstrapper : DefaultNancyBootstrapper
    {
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = WebConfigurationManager.AppSettings["nancy:DiagnosticsPassword"] }; }
        }
    }
}