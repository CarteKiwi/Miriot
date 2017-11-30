using System;
using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentView
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}
