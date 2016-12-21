using Miriot.Core.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Miriot.Services
{
    public class Authentication : IAuthentication
    {
        public async Task<WebAuthenticationResult> Login(WebAuthenticationOptions option, Uri brokerUri, Uri callBackUri)
        {
            return await WebAuthenticationBroker.AuthenticateAsync(option, brokerUri, callBackUri);
        }
    }
}
