using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace Miriot.Core.ViewModels.Widgets
{
    public class CalendarModel : WidgetModel
    {
        private string _token;
        private GraphUser _user;

        public string Token
        {
            get { return _token; }
            set { Set(() => Token, ref _token, value); }
        }

        public GraphUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public CalendarModel()
        {
            Title = "Calendrier & mails";
        }

        public override WidgetInfo GetInfos()
        {
            var vault = new PasswordVault();
            var passwordCredentials = vault.RetrieveAll();
            var temp = passwordCredentials.FirstOrDefault(c => c.Resource == "AccessToken");
            var cred = vault.Retrieve(temp.Resource, temp.UserName);

            return new OAuthWidgetInfo { Token = cred.Password, Username = temp.UserName };
        }

        public override async void LoadInfos(List<string> infos)
        {
            var info = infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            var cred = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info);

            if (!string.IsNullOrEmpty(cred.Token) || !string.IsNullOrEmpty(cred.TokenSecret))
            {
                var vault = new PasswordVault();
                var passwordCredential = new PasswordCredential("AccessToken", cred.Username, cred.Token);
                vault.Add(passwordCredential);
                ApplicationData.Current.LocalSettings.Values["user"] = cred.Username;
            }

            User = await ServiceLocator.Current.GetInstance<IGraphService>().GetUserAsync();
            base.LoadInfos(infos);
        }

        public override void OnActivated()
        {
            if (string.IsNullOrEmpty(Token))
            {
                var dispatcher = ServiceLocator.Current.GetInstance<IDispatcherService>();
                dispatcher.Invoke(async () => await Initialize());
            }
        }

        public override void OnDisabled()
        {
            Token = string.Empty;
            base.OnDisabled();
        }

        private async Task Initialize()
        {
            var auth = ServiceLocator.Current.GetInstance<IGraphService>();
            auth.Initialize();

            try
            {
                if (await auth.LoginAsync())
                {
                    User = await auth.GetUserAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                User = new GraphUser { Name = "Echec de la connexion" };
                IsActive = false;
            }
        }
    }
}
