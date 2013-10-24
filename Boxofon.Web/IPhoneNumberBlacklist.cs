namespace Boxofon.Web
{
    public interface IPhoneNumberBlacklist
    {
        bool Contains(string number);
    }
}