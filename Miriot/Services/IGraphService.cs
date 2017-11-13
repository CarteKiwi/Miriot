﻿using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IGraphService
    {
        void Initialize(OAuthWidgetInfo infos);

        bool IsInitialized { get; set; }

        Task<bool> LoginAsync();

        Task<GraphUser> GetUserAsync();

        Task LogoutAsync();

        Task<string> GetCodeAsync();

        Task AuthenticateForDeviceAsync();
    }
}
