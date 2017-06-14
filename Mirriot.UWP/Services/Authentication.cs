using Miriot.Core.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common.Model.Widgets;

namespace Miriot.Services
{
    public class Authentication : IAuthentication
    {
        public bool IsInitialized { get; set; }

        public bool Initialize(string appClientId)
        {
            return MicrosoftGraphService.Instance.Initialize(appClientId);
        }

        public async Task<WebAuthenticationResult> Login(WebAuthenticationOptions option, Uri brokerUri, Uri callBackUri)
        {
            return await WebAuthenticationBroker.AuthenticateAsync(option, brokerUri, callBackUri);
        }

        public Task<bool> LoginAsync()
        {
            return MicrosoftGraphService.Instance.LoginAsync();
        }

        public async Task<GraphUser> GetUserAsync()
        {
            var user = await MicrosoftGraphService.Instance.User.GetProfileAsync(CancellationToken.None);

            return new GraphUser {Name = user.DisplayName};
        }
    }
}
