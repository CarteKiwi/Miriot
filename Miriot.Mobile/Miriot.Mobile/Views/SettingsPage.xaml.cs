using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Core;
using Miriot.Services;
using Miriot.Core.ViewModels;
using Miriot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Miriot.Model;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class SettingsPage : ContentPage
    {
        public SettingsViewModel Vm { get; } = SimpleIoc.Default.GetInstance<SettingsViewModel>();

        public SettingsPage()
        {
            InitializeComponent();
            //BindingContext = Vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var b = BindingContext;
            //var nav = SimpleIoc.Default.GetInstance<INavigationService>();
            //var o = nav.GetAndRemoveParameters();

            //if (o != null)
            //{
            //    Vm.User = o as User;
            //}

            Vm.ActionLoaded.Execute(null);
        }
    }
}
