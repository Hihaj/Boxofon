namespace Boxofon.Web.MailCommands
{
    public class SendSms
    {
        public string SenderEmail { get; set; }
        public string SenderBoxofonNumber { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string Text { get; set; }
    }
}