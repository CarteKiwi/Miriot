using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Services.Twitter;
using Miriot.Core.Services.Interfaces;
using Tweet = Miriot.Common.Model.Widgets.Twitter.Tweet;
using TwitterUser = Miriot.Common.Model.Widgets.Twitter.TwitterUser;

namespace Miriot.Services
{
    public class TwitterWrapperService : ITwitterService
    {
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

        public async Task<TwitterUser> GetUserAsync()
        {
            try
            {
                if(!IsInitialized)
                    Initialize();

                // Get current user info
                var user = await TwitterService.Instance.GetUserAsync();

                return new TwitterUser{
                    Name = user.Name,
                    Id = user.Id,
                    ProfileImageUrl = user.ProfileImageUrl,
                    ScreenName = user.ScreenName
                };
            }
            catch (InvalidOperationException ex)
            {
                Initialize();
                return await GetUserAsync();
            }
        }

        public async Task<IEnumerable<Tweet>> GetUserTimeLineAsync(string screenName)
        {
            // Get user timeline
            var tweet = await TwitterService.Instance.GetUserTimeLineAsync(screenName, 50);

            return tweet.Select(t => new Tweet
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                Text = t.Text,
                User = new TwitterUser
                {
                    Name = t.User.Name,
                    Id = t.User.Id,
                    ProfileImageUrl = t.User.ProfileImageUrl,
                    ScreenName = t.User.ScreenName
                }
            });
        }

        public async Task<bool> TweetStatusAsync(string text)
        {
            // Post a tweet
            return await TwitterService.Instance.TweetStatusAsync(text);
        }

        //        public async Task<bool> TweetStatusAsync(string text)
        //        {
        //            var status = new TwitterStatus
        //            {
        //                Message = TweetText.Text,

        //                // Optional parameters defined by the Twitter "update" API
        //                // (they may all be null or false)

        //                DisplayCoordinates = true,
        //                InReplyToStatusId = "@ValidAccount",
        //                Latitude = validLatitude,
        //                Longitude = validLongitude,
        //                PlaceId = "df51dec6f4ee2b2c",   // As defined by Twitter
        //                PossiblySensitive = true,       // As defined by Twitter (nudity, violence, or medical procedures)
        //                TrimUser = true
        //            }

        //await TwitterService.Instance.TweetStatusAsync(status);

        public async Task<bool> TweetStatusAsync(string text, IRandomAccessStream stream)
        {
            // Post a tweet with a picture
            return await TwitterService.Instance.TweetStatusAsync(text, stream);
        }

        public async Task<IEnumerable<Tweet>> SearchAsync(string text)
        {
            // Search for a specific tag
            var tweets = await TwitterService.Instance.SearchAsync(text, 50);
            return tweets.Select(t => new Tweet
            {
                CreatedAt = t.CreatedAt,
                Id = t.Id,
                Text = t.Text,
                User = new TwitterUser
                {
                    Name = t.User.Name,
                    Id = t.User.Id,
                    ProfileImageUrl = t.User.ProfileImageUrl,
                    ScreenName = t.User.ScreenName
                }
            });
        }

        public async Task<IEnumerable<Tweet>> GetHomeTimelineAsync()
        {
            if (!IsInitialized)
                Initialize();

            // Get user timeline
            var tweet = await TwitterService.Instance.RequestAsync(new TwitterDataConfig
            {
                QueryType = TwitterQueryType.Home
            }, 50);

            return tweet.Select(t => new Tweet
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                Text = t.Text,
                User = new TwitterUser
                {
                    Name = t.User.Name,
                    Id = t.User.Id,
                    ProfileImageUrl = t.User.ProfileImageUrl,
                    ScreenName = t.User.ScreenName
                }
            });
        }
    }
}
