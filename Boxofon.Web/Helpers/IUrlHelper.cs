using System.Collections.Generic;

namespace Boxofon.Web.Helpers
{
    public interface IUrlHelper
    {
        string GetAbsoluteUrl(string path);
        string GetAbsoluteUrl(string path, IDictionary<string, string> queryParameters);
    }
}