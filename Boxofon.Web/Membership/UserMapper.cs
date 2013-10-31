using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace Boxofon.Web.Membership
{
    public class UserMapper : IUserMapper
    {
        private readonly IUserRepository _userRepository;

        public UserMapper(IUserRepository userRepository)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return _userRepository.GetById(identifier);
        }
    }
}