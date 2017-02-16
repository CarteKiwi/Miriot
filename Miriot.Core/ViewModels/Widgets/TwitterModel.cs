using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TwitterModel : WidgetModel
    {
        private string _token;

        public string Token
        {
            get { return _token; }
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

        public override void LoadInfos(List<string> infos)
        {
            var info = infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            Token = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info).Token;
            base.LoadInfos(infos);
        }

        public override void OnActivated()
        {
            if (string.IsNullOrEmpty(Token))
            {
                var dispatcher = ServiceLocator.Current.GetInstance<IDispatcherService>();
                dispatcher.Invoke(async () => await Login());
            }
        }

        public override void OnDisabled()
        {
            Token = string.Empty;
            base.OnDisabled();
        }

        private async Task Login()
        {
            var service = ServiceLocator.Current.GetInstance<ITwitterService>();
            var success = await service.LoginAsync();

            IsActive = success;
        }
    }
}
