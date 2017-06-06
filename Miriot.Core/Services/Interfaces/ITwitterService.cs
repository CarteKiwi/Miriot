﻿using Miriot.Common.Model.Widgets.Twitter;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface ITwitterService
    {
        bool IsInitialized { get; set; }

        void Initialize();

        Task<bool> LoginAsync();

        Task<TwitterUser> GetUserAsync();
    }
}
