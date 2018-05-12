using System;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Mobile.Views;
using Miriot.Services;
using Rg.Plugins.Popup.Services;

namespace Miriot.Mobile.Services
{
    public class TwitterService : ITwitterService
    {
        private TaskCompletionSource<bool> _tcs;

        public TwitterService()
        {
        }

        public bool IsInitialized { get; set; }

        public Task<TwitterUser> GetUserAsync()
        {
            throw new NotImplementedException();
        }

        public async void Initialize()
        {
            var appId = "n4J84SiGTLXHFh7F5mex5PGLZ";
            _tcs = new TaskCompletionSource<bool>();

            var popup = new PopupLoginView(new Uri("http://aka.ms/devicelogin"));
            popup.Disappearing += Popup_Disappearing;

            await PopupNavigation.Instance.PushAsync(popup, true);

            await _tcs.Task;
            //await MicrosoftGraphService.Instance.LoginAsync();
        }

        void Popup_Disappearing(object sender, EventArgs e)
        {
            _tcs.SetResult(true);
        }

        public Task<bool> LoginAsync()
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}
