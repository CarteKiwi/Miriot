using Miriot.Api.Interfaces;
using Miriot.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Api
{
    public class UserRepository : IUserRepository
    {
        public IEnumerable<User> All => throw new NotImplementedException();

        public void Delete(string id)
        {
            throw new NotImplementedException();
        }

        public bool DoesUserExist(string id)
        {
            throw new NotImplementedException();
        }

        public User Find(string id)
        {
            throw new NotImplementedException();
        }

        public void Insert(User user)
        {
            throw new NotImplementedException();
        }

        public void Update(User user)
        {
            throw new NotImplementedException();
        }
    }
}
