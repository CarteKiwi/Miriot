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
using Miriot.Common.Model;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class SettingsPage : ContentPage
    {
        public SettingsViewModel Vm { get; } = SimpleIoc.Default.GetInstance<SettingsViewModel>();

        public SettingsPage(User user)
        {
            InitializeComponent();
            BindingContext = Vm;
            Vm.User = user;
            Vm.ActionLoaded.Execute(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Vm.ActionLoaded.Execute(null);
        }
    }
}
