using System;
using System.Collections.Generic;
using System.Linq;

namespace Boxofon.Web.Membership
{
    public class InMemoryUserRepository : IUserRepository
    {
        private static readonly List<User> Users = new List<User>();
        private static readonly object SyncToken = new object();
        
        public User GetById(Guid id)
        {
            return Users.FirstOrDefault(user => user.Id == id);
        }

        public void Save(User user)
        {
            lock (SyncToken)
            {
                var existingUser = GetById(user.Id);
                if (existingUser != null)
                {
                    existingUser.Email = user.Email;
                    existingUser.TwilioAccountSid = user.TwilioAccountSid;
                    existingUser.ExternalIdentities.Clear();
                    foreach (var externalId in user.ExternalIdentities)
                    {
                        existingUser.ExternalIdentities.Add(externalId);
                    }
                }
                else
                {
                    Users.Add(user);
                }
            }
        }
    }
}