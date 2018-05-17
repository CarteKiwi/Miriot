using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Win10.Services
{
    public class GraphService : IGraphService
    {
        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            // From https://apps.dev.microsoft.com/#/appList
            //var appClientId = "59c7d308-7e47-4e5b-9f32-86cb2f3c8f88";

            // From Azure portal - Supinfo subscription
            var appClientId = "e57bfe1e-a88e-47f3-b47c-c414f8ca244b";
            IsInitialized = MicrosoftGraphService.Instance.Initialize(appClientId);
        }

        public async Task<string> GetCodeAsync()
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            await MicrosoftGraphService.Instance.InitializeDeviceCodeAsync();
            return MicrosoftGraphService.Instance.UserCode;
        }

        public async Task<bool> LoginAsync(bool hideError = false)
        {
            // Login via Azure Active Directory
            if (!await MicrosoftGraphService.Instance.LoginAsync())
            {
                if (!hideError)
                {
                    var error = new MessageDialog("Impossible de s'authentifier auprès d'Office 365");
                    await error.ShowAsync();
                }

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

        private async Task<byte[]> GetPhotoAsync()
        {
            try
            {
                using (IRandomAccessStream photoStream = await MicrosoftGraphService.Instance.User.GetPhotoAsync())
                {
                    if (photoStream == null)
                        return null;

                    using (var dr = new DataReader(photoStream.GetInputStreamAt(0)))
                    {
                        var bytes = new byte[photoStream.Size];
                        await dr.LoadAsync((uint)photoStream.Size);
                        dr.ReadBytes(bytes);
                        return bytes;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task LogoutAsync()
        {
            try
            {
                return MicrosoftGraphService.Instance.Logout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Task.FromResult(0);
        }

        public Task AuthenticateForDeviceAsync()
        {
            throw new NotImplementedException();
        }
    }
}
