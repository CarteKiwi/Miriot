using Miriot.Services;
using System;
using Xamarin.Forms;

namespace Miriot.iOS.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}
