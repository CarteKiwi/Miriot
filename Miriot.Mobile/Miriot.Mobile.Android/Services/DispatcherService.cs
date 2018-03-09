using GalaSoft.MvvmLight.Threading;
using Miriot.Services;
using System;

namespace Miriot.Droid.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
