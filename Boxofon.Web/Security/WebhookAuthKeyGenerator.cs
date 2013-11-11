using System.IO;

namespace Boxofon.Web.Security
{
    public class WebhookAuthKeyGenerator : IWebhookAuthKeyGenerator
    {
        public string GenerateAuthKey()
        {
            return (Path.GetRandomFileName() +
                    Path.GetRandomFileName() +
                    Path.GetRandomFileName() +
                    Path.GetRandomFileName() +
                    Path.GetRandomFileName()).Replace(".", string.Empty);
        }
    }
}