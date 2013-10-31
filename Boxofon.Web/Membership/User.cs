using System;
using System.Collections.Generic;
using Nancy.Security;

namespace Boxofon.Web.Membership
{
    public class User : IUserIdentity
    {
        public class ProviderIdentity
        {
            public string ProviderName { get; set; }
            public string ProviderUserId { get; set; }

            public ProviderIdentity()
            {
            }

            public ProviderIdentity(string providerName, string providerUserId)
            {
                ProviderName = providerName;
                ProviderUserId = providerUserId;
            }
        }

        private readonly List<ProviderIdentity> _providerIdentities = new List<ProviderIdentity>(); 

        public Guid Id { get; set; }
        public string Email { get; set; }
        public IList<ProviderIdentity> ProviderIdentities { get { return _providerIdentities; } } 
        
        string IUserIdentity.UserName
        {
            get { return Id.ToString(); }
        }

        IEnumerable<string> IUserIdentity.Claims
        {
            get { throw new NotImplementedException(); }
        }
    }
}