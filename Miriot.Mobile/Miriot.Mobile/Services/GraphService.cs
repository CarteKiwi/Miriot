using System;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets;
using Miriot.Mobile.Views;
using Miriot.Services;
using Rg.Plugins.Popup.Services;

namespace Miriot.Mobile.Services
{
    public class GraphService : IGraphService
    {
        public GraphService()
        {
        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
        }

        public async Task AuthenticateForDeviceAsync()
        {
            var popup = new PopupLoginView(new Uri("http://aka.ms/devicelogin"));

            await PopupNavigation.Instance.PushAsync(popup, true);
        }

        public Task<string> GetCodeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GraphUser> GetUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoginAsync(bool hideError = false)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
