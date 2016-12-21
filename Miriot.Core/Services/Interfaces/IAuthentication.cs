using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Miriot.Core.Services.Interfaces
{
    public interface IAuthentication
    {
        Task<WebAuthenticationResult> Login(WebAuthenticationOptions option, Uri brokerUri, Uri callBackUri);
    }
}
