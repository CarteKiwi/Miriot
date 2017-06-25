using Miriot.Core.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common.Model.Widgets;

namespace Miriot.Services
{
    public class GraphService : IGraphService
    {
        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            // From Azure portal - Cellenza subscription
            var appClientId = "ca026d51-8d86-4f85-a697-7be9c0a86453";

            IsInitialized = MicrosoftGraphService.Instance.Initialize(appClientId);
        }

        public async Task<bool> LoginAsync()
        {
            // Login via Azure Active Directory
            if (!await MicrosoftGraphService.Instance.LoginAsync())
            {
                var error = new MessageDialog("Unable to sign in to Office 365");
                await error.ShowAsync();
                return false;
            }

            return true;
        }

        public async Task<GraphUser> GetUserAsync()
        {
            try
            {
                if (!IsInitialized)
                    Initialize();

                var user = await MicrosoftGraphService.Instance.User.GetProfileAsync(CancellationToken.None);

                var photo = await GetPhotoAsync();

                return new GraphUser { Name = user.DisplayName, Photo = photo };
            }
            catch (InvalidOperationException)
            {
                Initialize();
                return await GetUserAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<BitmapImage> GetPhotoAsync()
        {
            using (IRandomAccessStream photoStream = await MicrosoftGraphService.Instance.User.GetPhotoAsync())
            {
                BitmapImage photo = new BitmapImage();
                if (photoStream != null)
                {
                    await photo.SetSourceAsync(photoStream);
                }
                else
                {
                    photo.UriSource = new Uri("ms-appx:///Assets/Square44x44Logo.scale-200.png");
                }

                return photo;
            }
        }
    }
}
