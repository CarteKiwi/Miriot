using Miriot.Common;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface ILuisService
    {
        Task<LuisResponse> AskLuisAsync(string words);
    }
}
