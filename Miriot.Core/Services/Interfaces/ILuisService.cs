using Miriot.Common;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface ILuisService
    {
        Task<LuisResponse> AskLuisAsync(string words);
    }
}
