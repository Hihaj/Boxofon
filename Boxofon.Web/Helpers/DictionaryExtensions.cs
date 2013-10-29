using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Helpers;

namespace Boxofon.Web.Helpers
{
    public static class DictionaryExtensions
    {
        public static string ToQueryString<TValue>(this IDictionary<string, TValue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return string.Empty;
            }
            var array = dictionary
                .Select(item => string.Format("{0}={1}", HttpUtility.UrlEncode(item.Key), HttpUtility.UrlEncode(item.Value.ToString())))
                .ToArray();
            return string.Join("&", array);
        }
    }
}