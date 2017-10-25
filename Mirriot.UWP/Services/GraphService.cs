using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Core.Services.Interfaces;
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

        public void Initialize(OAuthWidgetInfo infos)
        {
            if (!string.IsNullOrEmpty(infos.Code))
            {
                ApplicationData.Current.LocalSettings.Values["userCode"] = infos.Code;
            }

            if (!string.IsNullOrEmpty(infos.Token))
            {
                var vault = new PasswordVault();
                var passwordCredential = new PasswordCredential("AccessToken", infos.Username, infos.Token);
                vault.Add(passwordCredential);
                ApplicationData.Current.LocalSettings.Values["user"] = infos.Username;
            }

            // From Azure portal - Cellenza subscription
            //var appClientId = "ca026d51-8d86-4f85-a697-7be9c0a86453";

            // From Azure portal - Supinfo subscription
            var appClientId = "e57bfe1e-a88e-47f3-b47c-c414f8ca244b";
            //var appClientId = "1a383460-c136-44e4-be92-aa8a379f3265";
            IsInitialized = MicrosoftGraphService.Instance.Initialize(appClientId, MicrosoftGraphEnums.AuthenticationModel.V1);
        }

        public async Task GetCodeAsync()
        {
            // Login via Azure Active Directory
            //if (!await MicrosoftGraphService.Instance.GetCodeAsync())
            //{
            //    var error = new MessageDialog("Impossible de s'authentifier auprès d'Office 365");
            //    await error.ShowAsync();
            //    return false;
            //}
        }

        public async Task<bool> LoginAsync()
        {
            // Login via Azure Active Directory
            if (!await MicrosoftGraphService.Instance.LoginAsync())
            {
                var error = new MessageDialog("Impossible de s'authentifier auprès d'Office 365");
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

        private void Initialize()
        {
            Initialize(null);
        }

        private async Task<byte[]> GetPhotoAsync()
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

        public Task LogoutAsync()
        {
            try
            {
                return MicrosoftGraphService.Instance.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Task.FromResult(0);
        }
    }
}
