using Miriot.Common.Model;
using System.Collections.Generic;

namespace Miriot.Api.Interfaces
{
    public interface IUserRepository
    {
        bool DoesUserExist(string id);
        IEnumerable<User> All { get; }
        User Find(string id);
        void Insert(User user);
        void Update(User user);
        void Delete(string id);
    }
}
