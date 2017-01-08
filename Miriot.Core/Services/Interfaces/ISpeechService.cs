using System.IO;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface ISpeechService
    {
        Task<Stream> GetStream(string text);
    }
}
