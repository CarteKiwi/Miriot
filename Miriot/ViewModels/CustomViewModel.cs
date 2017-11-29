using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels
{
    public abstract class CustomViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
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

        private RelayCommand _navigateBackCommand;

        public RelayCommand NavigateBackCommand
        {
            get
            {
                if (_navigateBackCommand == null)
                {
                    _navigateBackCommand = new RelayCommand(OnBack);
                }

                return _navigateBackCommand;
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

        private void OnBack()
        {
            _navigationService.GoBack();
        }

        public CustomViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
    }
}
