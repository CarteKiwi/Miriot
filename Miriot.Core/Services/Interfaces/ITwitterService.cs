using Miriot.Common.Model.Widgets.Twitter;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Miriot.Core.Services.Interfaces
{
    public interface ITwitterService
    {
        bool IsInitialized { get; set; }

        void Initialize();

        Task<bool> LoginAsync();

        Task<TwitterUser> GetUserAsync();

        Task<IEnumerable<Tweet>> GetUserTimeLineAsync(string screenName);

        Task<bool> TweetStatusAsync(string text);

        Task<bool> TweetStatusAsync(string text, IRandomAccessStream stream);
        Task<IEnumerable<Tweet>> SearchAsync(string text);

        Task<IEnumerable<Tweet>> GetHomeTimelineAsync();
    }
}
