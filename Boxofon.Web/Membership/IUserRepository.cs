using System;

namespace Boxofon.Web.Membership
{
    public interface IUserRepository
    {
        User GetById(Guid id);
        void Save(User user);
    }
}