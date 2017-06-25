using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Miriot.Common.Model.Widgets.Twitter;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel
    {
        private TwitterUser _user;

        public TwitterUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public TwitterModel()
        {
            Title = "Twitter";
        }

        public override WidgetInfo GetInfos()
        {
            var vault = new PasswordVault();
            var passwordCredentials = vault.RetrieveAll();
            var temp = passwordCredentials.FirstOrDefault(c => c.Resource == "TwitterAccessToken");
            var cred = vault.Retrieve(temp.Resource, temp.UserName);

            return new OAuthWidgetInfo { Token = cred.UserName, TokenSecret = cred.Password, Username = User.ScreenName };
        }

        public override async void LoadInfos(List<string> infos)
        {
            var info = infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            var cred = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info);

            if (!string.IsNullOrEmpty(cred.Token) || !string.IsNullOrEmpty(cred.TokenSecret))
            {
                var vault = new PasswordVault();
                var passwordCredential = new PasswordCredential("TwitterAccessToken", cred.Token, cred.TokenSecret);
                vault.Add(passwordCredential);
                ApplicationData.Current.LocalSettings.Values["TwitterScreenName"] = cred.Username;
            }

            User = await ServiceLocator.Current.GetInstance<ITwitterService>().GetUserAsync();

            base.LoadInfos(infos);
        }

        public override void OnActivated()
        {
            if (User != null) return;
            var dispatcher = ServiceLocator.Current.GetInstance<IDispatcherService>();
            dispatcher.Invoke(async () =>
            {
                await Login();
                await GetUser();
            });
        }

        public override void OnDisabled()
        {
            ServiceLocator.Current.GetInstance<ITwitterService>().Logout();
            base.OnDisabled();
        }

        private async Task Login()
        {
            var success = await ServiceLocator.Current.GetInstance<ITwitterService>().LoginAsync();
            IsActive = success;
        }

        private async Task GetUser()
        {
            User = await ServiceLocator.Current.GetInstance<ITwitterService>().GetUserAsync();
        }
    }
}
