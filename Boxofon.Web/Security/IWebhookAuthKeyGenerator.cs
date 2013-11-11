namespace Boxofon.Web.Security
{
    public interface IWebhookAuthKeyGenerator
    {
        string GenerateAuthKey();
    }
}