using System;
using Nancy.Session;
using ServiceStack.Text;

namespace Boxofon.Web.Helpers
{
    public static class SessionExtensions
    {
        public static void SetSessionValue<T>(this ISession session, string key, T entry)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("key must have a value", "key");
            }
            session[key] = JsonSerializer.SerializeToString(entry);
        }

        public static T GetSessionValue<T>(this ISession session, string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("key must have a value", "key");
            }

            var sessionItem = session[key];

            if (sessionItem == null)
            {
                return default(T);
            }

            return JsonSerializer.DeserializeFromString<T>(sessionItem.ToString());
        }
    }
}