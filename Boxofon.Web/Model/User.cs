using System;
using System.Collections.Generic;
using Nancy.Security;

namespace Boxofon.Web.Model
{
    public class User : IUserIdentity
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string TwilioAccountSid { get; set; }
        public List<ExternalIdentity> ExternalIdentities { get; set; }
        public string TwilioPhoneNumber { get; set; }
        public string PrivatePhoneNumber { get; set; } 
        
        string IUserIdentity.UserName
        {
            get { return Id.ToString(); }
        }

        IEnumerable<string> IUserIdentity.Claims
        {
            get { throw new NotSupportedException(); }
        }

        public User()
        {
            ExternalIdentities = new List<ExternalIdentity>();
        }
    }
}