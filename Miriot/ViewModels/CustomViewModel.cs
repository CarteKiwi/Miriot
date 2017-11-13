using GalaSoft.MvvmLight;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels
{
    public abstract class CustomViewModel : ViewModelBase
    {
        private object _parameter;
        protected object Parameter => _parameter;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        private string _isLoadingMessage;
        public string IsLoadingMessage
        {
            get { return _isLoadingMessage; }
            set
            {
                _isLoadingMessage = value;
                RaisePropertyChanged(() => IsLoadingMessage);
            }
        }

        protected abstract Task InitializeAsync();

        public async void Initialize(object parameter = null)
        {
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
