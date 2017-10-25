using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Core.Services.Interfaces;
using Miriot.Services.Interfaces;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel
    {
        public override string Title => "Twitter";

        public override WidgetType Type => WidgetType.Twitter;

        private TwitterUser _user;

        public TwitterUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public TwitterModel(Widget widget) : base(widget)
        {
        }

        public override WidgetInfo GetInfos()
        {
            var sec = SimpleIoc.Default.GetInstance<ISecurityService>();
            return sec.GetSecureData("TwitterAccessToken");
        }

        public override async Task LoadInfos()
        {
            var info = _infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            var cred = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info);

            if (!string.IsNullOrEmpty(cred.Token) || !string.IsNullOrEmpty(cred.TokenSecret))
            {
                //var vault = new PasswordVault();
                //var passwordCredential = new PasswordCredential("TwitterAccessToken", cred.Token, cred.TokenSecret);
                //vault.Add(passwordCredential);
                //ApplicationData.Current.LocalSettings.Values["TwitterScreenName"] = cred.Username;
            }

            User = await SimpleIoc.Default.GetInstance<ITwitterService>().GetUserAsync();
        }

        public override void OnActivated()
        {
            if (User != null) return;
            var dispatcher = SimpleIoc.Default.GetInstance<IDispatcherService>();
            dispatcher.Invoke(async () =>
            {
                await Login();
                await GetUser();
            });
        }

        public override void OnDisabled()
        {
            var service = SimpleIoc.Default.GetInstance<ITwitterService>();
            service.Logout();
        }

        private async Task Login()
        {
            var success = await SimpleIoc.Default.GetInstance<ITwitterService>().LoginAsync();
            IsActive = success;
        }

        private async Task GetUser()
        {
            User = await SimpleIoc.Default.GetInstance<ITwitterService>().GetUserAsync();
        }
    }
}
