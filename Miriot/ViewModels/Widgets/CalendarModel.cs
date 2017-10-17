using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
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
            set { Set(ref _token, value); }
        }

        public GraphUser User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public override string Title => "Calendrier & mails";

        public override WidgetType Type => WidgetType.Calendar;

        public CalendarModel(Widget widget) : base(widget)
        {
        }

        public override WidgetInfo GetInfos()
        {
            var vault = new PasswordVault();
            var passwordCredentials = vault.RetrieveAll();
            var temp = passwordCredentials.FirstOrDefault(c => c.Resource == "AccessToken");
            var cred = vault.Retrieve(temp.Resource, temp.UserName);

            return new OAuthWidgetInfo { Token = cred.Password, Username = temp.UserName };
        }

        public override async Task LoadInfos()
        {
            var info = _infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            var cred = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info);

            if (!string.IsNullOrEmpty(cred.Token))
            {
                var vault = new PasswordVault();
                var passwordCredential = new PasswordCredential("AccessToken", cred.Username, cred.Token);
                vault.Add(passwordCredential);
                //ApplicationData.Current.LocalSettings.Values["user"] = cred.Username;
                await Initialize();
            }
        }

        public override void OnActivated()
        {
            if (User == null)
            {
                var dispatcher = SimpleIoc.Default.GetInstance<IDispatcherService>();
                dispatcher.Invoke(async () => await Initialize());
            }
        }

        public override async void OnDisabled()
        {
            User = null;

            try
            {
                var auth = SimpleIoc.Default.GetInstance<IGraphService>();
                await auth.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task Initialize()
        {
            var auth = SimpleIoc.Default.GetInstance<IGraphService>();
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
                //User = new GraphUser { Name = "Echec de la connexion" };
                User = null;
                IsActive = false;
            }
        }
    }
}
