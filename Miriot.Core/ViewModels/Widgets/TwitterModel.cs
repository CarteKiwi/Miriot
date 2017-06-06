using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets.Twitter;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel
    {
        private string _token;
        private TwitterUser _user;

        public TwitterUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public string Token
        {
            get => _token;
            set { Set(() => Token, ref _token, value); }
        }

        public TwitterModel()
        {
            Title = "Twitter";
        }

        public override WidgetInfo GetInfos()
        {
            return new OAuthWidgetInfo { Token = Token };
        }

        public override async void LoadInfos(List<string> infos)
        {
            var info = infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            Token = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info).Token;
            User = await ServiceLocator.Current.GetInstance<ITwitterService>().GetUserAsync();

            base.LoadInfos(infos);
        }

        public override void OnActivated()
        {
            if (!string.IsNullOrEmpty(Token)) return;
            var dispatcher = ServiceLocator.Current.GetInstance<IDispatcherService>();
            dispatcher.Invoke(async () =>
            {
                await Login();
                await GetUser();
            });
        }

        public override void OnDisabled()
        {
            Token = string.Empty;
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
