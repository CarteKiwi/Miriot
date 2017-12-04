using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Services;
using Miriot.Services.Interfaces;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel<OAuthWidgetInfo>
    {
        public override string Title => "Twitter";

        public override WidgetStates State => WidgetStates.Compact;

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

        public override OAuthWidgetInfo GetModel()
        {
            var sec = SimpleIoc.Default.GetInstance<ISecurityService>();
            return sec.GetSecureData("TwitterAccessToken");
        }

        public override async Task Load()
        {
            if (!string.IsNullOrEmpty(Model?.Token) || !string.IsNullOrEmpty(Model?.TokenSecret))
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
