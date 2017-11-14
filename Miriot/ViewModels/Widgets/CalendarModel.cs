using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Services;
using Miriot.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class CalendarModel : WidgetModel
    {
        private OAuthWidgetInfo _authInfos;
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
            var sec = SimpleIoc.Default.GetInstance<ISecurityService>();
            return sec.GetSecureData("AccessToken");

            //var vault = new PasswordVault();
            //var passwordCredentials = vault.RetrieveAll();
            //var temp = passwordCredentials.FirstOrDefault(c => c.Resource == "AccessToken");
            //var cred = vault.Retrieve(temp.Resource, temp.UserName);

            //var temp2 = passwordCredentials.FirstOrDefault(c => c.Resource == "UserCode");
            //var cred2 = vault.Retrieve(temp2.Resource, temp2.UserName);

            //return new OAuthWidgetInfo { Token = cred.Password, Username = temp.UserName, Code = cred2.Password };
        }

        public override async Task LoadInfos()
        {
            var info = _infos?.FirstOrDefault();
            if (string.IsNullOrEmpty(info) || info == "null") return;

            _authInfos = JsonConvert.DeserializeObject<OAuthWidgetInfo>(info);

            await Initialize();
        }

        public override void OnActivated()
        {
            if (User == null)
            {
                var dispatcher = SimpleIoc.Default.GetInstance<IDispatcherService>();
                dispatcher.Invoke(async () =>
                {
                    var rome = SimpleIoc.Default.GetInstance<IRomeService>();
                    var auth = SimpleIoc.Default.GetInstance<IGraphService>();

                    // Tell the mirror to display code
                    await rome.CommandAsync("GraphService_Initialize");

                    // Redirect the user to the login page
                    await auth.AuthenticateForDeviceAsync();

                    // Tell the mirror to retrieve user
                    User = await rome.CommandAsync<GraphUser>("GraphService_GetUser");
                });
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
            auth.Initialize(_authInfos);

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
