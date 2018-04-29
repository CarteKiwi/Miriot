using System;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets;
using Miriot.Common.Model.Widgets.Twitter;
using Miriot.Services;

namespace Miriot.iOS.Services
{
    public class GraphService : IGraphService
    {
        public bool IsInitialized { get; set; }

        public Task AuthenticateForDeviceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCodeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GraphUser> GetUserAsync()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public Task<bool> LoginAsync(bool hideError = false)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
