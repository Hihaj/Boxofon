namespace Boxofon.Web.Model
{
    public interface ITwilioPhoneNumberRepository
    {
        TwilioPhoneNumber GetByPhoneNumber(string phoneNumber);
        void Save(TwilioPhoneNumber phoneNumber);
    }
}