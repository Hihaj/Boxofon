using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using System.Linq;
using System.Text;
using Nancy;

namespace Boxofon.Web
{
    public class FilePhoneNumberBlacklist : IPhoneNumberBlacklist
    {
        private readonly HashSet<string> _phoneNumbers;

        public FilePhoneNumberBlacklist(IRootPathProvider rootPathProvider)
        {
            if (rootPathProvider == null)
            {
                throw new ArgumentNullException("rootPathProvider");
            }
            _phoneNumbers = new HashSet<string>(File.ReadAllLines(Path.Combine(rootPathProvider.GetRootPath(), @"App_Data\blacklist.txt"), Encoding.UTF8)
                                                    .Select(line => line.Trim())
                                                    .Where(line => !string.IsNullOrEmpty(line)));
        }

        public bool Contains(string number)
        {
            return _phoneNumbers.Contains(number);
        }
    }
}