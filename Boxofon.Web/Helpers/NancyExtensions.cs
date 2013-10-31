using Boxofon.Web.Infrastructure;
using Boxofon.Web.Membership;
using Nancy;

namespace Boxofon.Web.Helpers
{
    public static class NancyExtensions
    {
        public static bool IsAuthenticated(this NancyModule module)
        {
            return module.Context.CurrentUser != null;
        }

        public static User GetCurrentUser(this NancyModule module)
        {
            return module.Context.CurrentUser as User;
        }

        public static void AddAlertMessage(this Request request, string messageType, string alertMessage)
        {
            var container = request.Session.GetSessionValue<AlertMessageStore>(AlertMessageStore.AlertMessageKey);

            if (container == null)
            {
                container = new AlertMessageStore();
            }

            container.AddMessage(messageType, alertMessage);

            request.Session.SetSessionValue(AlertMessageStore.AlertMessageKey, container);
        }
    }
}