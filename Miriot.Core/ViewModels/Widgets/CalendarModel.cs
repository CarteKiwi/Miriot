using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Miriot.Common.Model.Widgets;

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
            var auth = ServiceLocator.Current.GetInstance<IAuthentication>();
            // from Azure portal - Cellenza subscription
            var clientId = "ca026d51-8d86-4f85-a697-7be9c0a86453";

            if (auth.Initialize(clientId))
            {
                if (await auth.LoginAsync())
                {
                    User = await auth.GetUserAsync();
                }
            }
        }

        private async Task Login()
        {
            var auth = ServiceLocator.Current.GetInstance<IAuthentication>();

            var clientId = "1a383460-c136-44e4-be92-aa8a379f3265";
            var secret = "mQVbkao2vkhptmAOerCsfH4";
            var redirectUri = "https://miriot.suismoi.fr";
            var scopes = "https://outlook.office.com/mail.readwrite https://outlook.office.com/calendars.read";
            var startUri = new Uri($"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={WebUtility.UrlEncode(scopes)}");
            var webAuthenticationResult = await auth.Login(WebAuthenticationOptions.None, startUri, new Uri("https://miriot.suismoi.fr"));

            if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var uriWithCode = webAuthenticationResult.ResponseData;

                var parameter = uriWithCode.Split('&').FirstOrDefault(e => e.Contains("code"));
                var code = parameter.Replace($"{redirectUri}/?code=", "");

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/");

                    var content = new FormUrlEncodedContent(new[]
                   {
                        new KeyValuePair<string,string>("grant_type", "authorization_code"),
                        new KeyValuePair<string,string>("client_id", clientId),
                        new KeyValuePair<string,string>("client_secret", secret),
                        new KeyValuePair<string,string>("code", code),
                        new KeyValuePair<string,string>("redirect_uri", redirectUri)
                    });

                    var res = await client.PostAsync("token", content);
                    var contentResponse = await res.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<AzurePayLoad>(contentResponse).access_token;

                    Token = token;
                }
            }
            else
            {
                IsActive = false;
            }
        }
    }

    public class AzurePayLoad
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string access_token { get; set; }
        public string scope { get; set; }
    }
}
