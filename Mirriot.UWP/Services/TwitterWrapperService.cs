using Microsoft.Toolkit.Uwp.Services.Twitter;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TwitterUser = Miriot.Common.Model.Widgets.Twitter.TwitterUser;

namespace Miriot.Win10.Services
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
            TwitterService.Instance.Initialize("n4J84SiGTLXHFh7F5mex5PGLZ", "8ht8N38Sh8hrNYgww3XRYS8X6gIcoywFoJYDcAoBoSfZXaKibt", "https://Miriot.Win10.suismoi.fr");

            IsInitialized = true;
        }

        public async Task<bool> LoginAsync()
        {
            // Login to Twitter
            return await TwitterService.Instance.LoginAsync();
        }

        public void Logout()
        {
            try
            {
                TwitterService.Instance.Logout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
