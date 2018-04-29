using System;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Services;

namespace Miriot.iOS.Services
{
    public class TwitterService : ITwitterService
    {
        public TwitterService()
        {
        }

        public bool IsInitialized { get; set; }

        public Task<TwitterUser> GetUserAsync()
        {
            return Task.FromResult(new TwitterUser());
        }

        public void Initialize()
        {
        }

        public Task<bool> LoginAsync()
        {
            return Task.FromResult(false);
        }

        public void Logout()
        {
        }
    }
}
