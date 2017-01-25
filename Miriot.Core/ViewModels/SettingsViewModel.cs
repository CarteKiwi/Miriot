using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels.Widgets;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Miriot.Core.Services;

namespace Miriot.Core.ViewModels
{
    public class SettingsViewModel : CustomViewModel
    {
        #region Commands
        public RelayCommand ActionLoaded { get; set; }
        public RelayCommand ActionSave { get; set; }
        public RelayCommand ActionDelete { get; set; }
        #endregion

        #region Variables
        private readonly IDialogService _dialogService;
        private readonly FaceService _faceService;
        private readonly IDispatcherService _dispatcher;
        private User _user;
        private ObservableCollection<WidgetModel> _widgets;
        #endregion

        #region Properties
        public ObservableCollection<WidgetModel> Widgets
        {
            get { return _widgets; }
            set { Set(() => Widgets, ref _widgets, value); }
        }

        public User User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }
        #endregion

        public SettingsViewModel(
            IDialogService dialogService, 
            IDispatcherService dispatcher)
        {
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            _faceService = new FaceService();

            ActionLoaded = new RelayCommand(OnLoaded);
            ActionSave = new RelayCommand(OnSave);
            ActionDelete = new RelayCommand(async () => await OnDelete());
        }

        private async Task OnDelete()
        {
            var isSuccess = await _faceService.DeletePerson(User.Id);

            if (isSuccess)
                _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Utilisateur supprimé", "Information"));
            else
                _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Erreur lors de la suppression. Réessayez.", "Alerte"));
        }

        private void OnSave()
        {
            foreach (var w in Widgets)
            {
                AddRemoveWidget(w);
            }

            Task.Run(async () =>
            {
                var isSuccess = await UpdateUserAsync();

                if (isSuccess)
                    _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Modifications sauvegardées", "Information"));
                else
                    _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Erreur lors de la sauvegarde. Réessayez.", "Alerte"));
            });
        }

        private async Task<bool> UpdateUserAsync()
        {
            var vm = ServiceLocator.Current.GetInstance<MainViewModel>();
            return await vm.UpdateUserAsync();
        }

        private void AddRemoveWidget(WidgetModel w)
        {
            // If desactivated
            if (!w.IsActive)
            {
                var ww = User.UserData.Widgets.FirstOrDefault(e => e.Type == w.WidgetType);

                // Should be existing
                if (ww != null)
                {
                    // Remove
                    User.UserData.Widgets.Remove(ww);
                }
            }
            else // if activated
            {
                var ww = User.UserData.Widgets.FirstOrDefault(e => e.Type == w.WidgetType);

                // Should not be existing
                if (ww == null)
                    User.UserData.Widgets.Add(w.ToWidget());
                else
                {
                    var newW = w.ToWidget();
                    ww.Infos = newW.Infos;
                    ww.X = newW.X;
                    ww.Y = newW.Y;
                }
            }
        }

        private void OnLoaded()
        {
            User = ServiceLocator.Current.GetInstance<MainViewModel>().User;

            if (User == null)
                return;

            Widgets = new ObservableCollection<WidgetModel>();

            foreach (var type in Enum.GetNames(typeof(WidgetType)))
            {
                var wt = (WidgetType)Enum.Parse(typeof(WidgetType), type);

                WidgetModel w;

                switch (wt)
                {
                    case WidgetType.Weather:
                        w = new WeatherModel();
                        break;
                    case WidgetType.Calendar:
                        w = new CalendarModel();
                        break;
                    case WidgetType.Horoscope:
                        w = new HoroscopeModel();
                        break;
                    default:
                        w = new WidgetModel();
                        break;
                }

                w.WidgetType = (WidgetType)Enum.Parse(typeof(WidgetType), type);
                var ww = User.UserData.Widgets.FirstOrDefault(e => e.Type == w.WidgetType);

                if (ww != null)
                {
                    w.X = ww.X;
                    w.Y = ww.Y;
                    w.LoadInfos(ww.Infos);

                    w.SetActive();
                }
                else
                    w.IsActive = false;

                Widgets.Add(w);
            }
        }
    }

}
