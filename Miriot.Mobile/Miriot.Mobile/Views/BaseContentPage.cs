using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Miriot.Mobile.Views
{
    public abstract class BaseContentPage : ContentPage
    {

    }

    public abstract class BaseContentPage<T> : BaseContentPage
        where T : CustomViewModel
    {
        protected T ViewModel { get; set; }

        public BaseContentPage()
        {
            ViewModel = SimpleIoc.Default.GetInstance<T>();
            BindingContext = ViewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ViewModel_OnPropertyChanged(e.PropertyName);
        }

        public virtual void ViewModel_OnPropertyChanged(string propertyName)
        {

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.Initialize();
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.NavigateBackCommand.Execute(null);
            return false;// base.OnBackButtonPressed();
        }
    }
}