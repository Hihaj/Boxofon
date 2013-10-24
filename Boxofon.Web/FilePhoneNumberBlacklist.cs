using System.Collections.Generic;
using System.IO;
using System.Web;

using System.Linq;
using System.Text;

namespace Boxofon.Web
{
    public class FilePhoneNumberBlacklist : IPhoneNumberBlacklist
    {
        private const string BlacklistFilePath = @"~/App_Data/blacklist.txt";
        private readonly HashSet<string> _phoneNumbers;

        public FilePhoneNumberBlacklist(HttpContextBase httpContext)
        {
            _phoneNumbers = new HashSet<string>(File.ReadAllLines(httpContext.Server.MapPath(BlacklistFilePath), Encoding.UTF8)
                                                    .Select(line => line.Trim())
                                                    .Where(line => !string.IsNullOrEmpty(line)));
        }

        public bool Contains(string number)
        {
            return _phoneNumbers.Contains(number);
        }
    }
}