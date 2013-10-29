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
            return GetAbsoluteUrl(path, null);
        }

        public string GetAbsoluteUrl(string path, IDictionary<string, string> queryParameters)
        {
            var url = new UriBuilder(BaseUrl)
            {
                Path = path,
                Query = queryParameters.ToQueryString()
            };
            return url.Uri.AbsoluteUri;
        }
    }
}