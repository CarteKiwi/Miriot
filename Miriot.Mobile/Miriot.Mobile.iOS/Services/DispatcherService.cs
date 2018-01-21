using GalaSoft.MvvmLight.Threading;
using Miriot.Services;
using System;

namespace Miriot.iOS.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
