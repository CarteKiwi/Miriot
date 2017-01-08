using System;
using System.Threading.Tasks;
using Miriot.Common;
using Miriot.Common.Model;

namespace Miriot.Core.Services.Interfaces
{
    public interface IFaceService
    {
        Task<ServiceResponse> GetUsers(byte[] bitmap);

        Task<UserEmotion> GetEmotion(byte[] bitmap, int top, int left);

        Task<bool> DeletePerson(Guid personId);

        Task<bool> CreatePerson(byte[] bitmap, string name);

        Task<bool> UpdatePerson(User user, byte[] pic);
    }
}
