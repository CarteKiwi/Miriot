using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Miriot.Common.Model.Widgets;

namespace Miriot.Core.Services.Interfaces
{
    public interface IAuthentication
    {
        Task<WebAuthenticationResult> Login(WebAuthenticationOptions option, Uri brokerUri, Uri callBackUri);
        bool Initialize(string appClientId);

        bool IsInitialized { get; set; }

        Task<bool> LoginAsync();

        Task<GraphUser> GetUserAsync();
    }
}
