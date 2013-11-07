using System;
using System.Collections.Generic;
using Nancy.Security;

namespace Boxofon.Web.Membership
{
    public class User : IUserIdentity
    {
        private readonly List<ExternalIdentity> _externalIdentities = new List<ExternalIdentity>(); 

        public Guid Id { get; set; }
        public string Email { get; set; }
        public string TwilioAccountSid { get; set; }
        public IList<ExternalIdentity> ExternalIdentities { get { return _externalIdentities; } } 
        
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