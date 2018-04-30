using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Services.MicrosoftGraph;
using Miriot.Common.Model.Widgets;
using Miriot.Services;

namespace Miriot.Mobile.Services
{
    public class GraphService : IGraphService
    {
        public GraphService()
        {
        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            // From Azure portal - Supinfo subscription
            var appClientId = "e57bfe1e-a88e-47f3-b47c-c414f8ca244b";
            IsInitialized = MicrosoftGraphService.Instance.Initialize(appClientId);
        }

        public Task AuthenticateForDeviceAsync()
        {
            return null;
        }

        public Task<string> GetCodeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GraphUser> GetUserAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LoginAsync(bool hideError = false)
        {
            throw new NotImplementedException();
            // return await MicrosoftGraphService.Instance.LoginAsync();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
