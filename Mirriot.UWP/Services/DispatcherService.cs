using Miriot.Core.Services.Interfaces;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Miriot.Services
{
    public class DispatcherService : IDispatcherService
    {
        public async void Invoke(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action.Invoke());
        }
    }
}
