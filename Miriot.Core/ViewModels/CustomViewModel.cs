using GalaSoft.MvvmLight;

namespace Miriot.Core.ViewModels
{
    public class CustomViewModel : ViewModelBase
    {
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
    }
}
