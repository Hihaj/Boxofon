namespace Boxofon.Web.ViewModels
{
    public class AccountOverview
    {
        public bool IsTwilioAccountConnected { get; set; }
        public string TwilioConnectAuthorizationUrl { get; set; }
        public string BoxofonNumber { get; set; }
        public string PrivatePhoneNumber { get; set; }
        public string Email { get; set; }
    }
}