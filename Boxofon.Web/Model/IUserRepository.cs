using System;

namespace Boxofon.Web.Model
{
    public interface IUserRepository
    {
        User GetById(Guid id);
        void Save(User user);
    }
}