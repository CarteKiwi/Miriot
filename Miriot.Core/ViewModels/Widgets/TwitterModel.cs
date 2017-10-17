using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Twitter;

        private TwitterUser _user;

        public TwitterUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public TwitterModel(Widget widget) : base(widget)
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

        public override async Task LoadInfos()
        {
            var info = _infos?.FirstOrDefault();
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
            var service = ServiceLocator.Current.GetInstance<ITwitterService>();
            service.Logout();
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
