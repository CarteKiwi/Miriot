﻿using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common.Model.Widgets;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace Miriot.Win10.Services
{
    public class GraphService : IGraphService
    {
        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            // From Azure portal - Cellenza subscription
            //var appClientId = "ca026d51-8d86-4f85-a697-7be9c0a86453";

            // From Azure portal - Supinfo subscription
            var appClientId = "e57bfe1e-a88e-47f3-b47c-c414f8ca244b";
            //var appClientId = "1a383460-c136-44e4-be92-aa8a379f3265";
            IsInitialized = MicrosoftGraphService.Instance.Initialize(appClientId, MicrosoftGraphEnums.AuthenticationModel.V1);
        }

        public Task AuthenticateForDeviceAsync()
        {
            return MicrosoftGraphService.Instance.AuthenticateForDeviceAsync();
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
                return MicrosoftGraphService.Instance.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Task.FromResult(0);
        }

        public async Task<string> GetCodeAsync()
        {
            await MicrosoftGraphService.Instance.InitializeForDeviceCodeAsync();
            return MicrosoftGraphService.Instance.UserCode;
        }
    }
}
