using TinyMessenger;

namespace Boxofon.Web.Infrastructure
{
    public interface ISubscriber
    {
        void RegisterSubscriptions(ITinyMessengerHub hub);
    }
}