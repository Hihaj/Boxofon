using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Nancy.Helpers;

namespace Boxofon.Web.Helpers
{
    public class UrlHelper : IUrlHelper
    {
        private static readonly string BaseUrl = WebConfigurationManager.AppSettings["boxofon:BaseUrl"];

        public string GetAbsoluteUrl(string path)
        {
            var url = new UriBuilder(BaseUrl)
            {
                Path = path
            };
            return url.Uri.AbsoluteUri;
        }

        public string GetAbsoluteUrl(string path, IDictionary<string, string> queryParameters)
        {
            var url = new UriBuilder(BaseUrl)
            {
                Path = path
            };
            if (queryParameters != null && queryParameters.Any())
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                foreach (var queryParameter in queryParameters)
                {
                    query[queryParameter.Key] = queryParameter.Value;
                }
                url.Query = query.ToString();
            }
            return url.Uri.AbsoluteUri;
        }
    }
}