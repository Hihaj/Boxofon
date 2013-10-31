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

        public Guid? GetIdByProviderNameAndProviderUserId(string providerName, string providerUserId)
        {
            return Users
                .Where(user => user.ProviderIdentities.Any(pid => pid.ProviderName == providerName && pid.ProviderUserId == providerUserId))
                .Select(user => (Guid?)user.Id)
                .FirstOrDefault();
        }

        public void Save(User user)
        {
            lock (SyncToken)
            {
                var existingUser = GetById(user.Id);
                if (existingUser != null)
                {
                    existingUser.Email = user.Email;
                    // TODO Update provider identitites
                }
                else
                {
                    Users.Add(user);
                }
            }
        }
    }
}