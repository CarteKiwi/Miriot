using Microsoft.Toolkit.Uwp.Services.Twitter;
using Miriot.Core.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using TwitterUser = Miriot.Common.Model.Widgets.Twitter.TwitterUser;

namespace Miriot.Services
{
    public class TwitterWrapperService : ITwitterService
    {
        public TwitterWrapperService()
        {

        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            // Initialize service
            TwitterService.Instance.Initialize("n4J84SiGTLXHFh7F5mex5PGLZ", "8ht8N38Sh8hrNYgww3XRYS8X6gIcoywFoJYDcAoBoSfZXaKibt", "https://miriot.suismoi.fr");

            IsInitialized = true;
        }

        public async Task<bool> LoginAsync()
        {
            // Login to Twitter
            return await TwitterService.Instance.LoginAsync();
        }

        public void Logout()
        {
            TwitterService.Instance.Logout();
        }

        public async Task<TwitterUser> GetUserAsync()
        {
            try
            {
                if (!IsInitialized)
                    Initialize();

                // Get current user info
                var user = await TwitterService.Instance.GetUserAsync();

                return new TwitterUser
                {
                    Name = user.Name,
                    Id = user.Id,
                    ProfileImageUrl = user.ProfileImageUrl,
                    ScreenName = user.ScreenName
                };
            }
            catch (InvalidOperationException)
            {
                Initialize();
                return await GetUserAsync();
            }
        }
    }
}
