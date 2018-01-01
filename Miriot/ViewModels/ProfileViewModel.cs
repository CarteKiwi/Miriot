using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels
{
    public class ProfileViewModel : CustomViewModel
    {
        #region Commands
        public RelayCommand ActionEditName { get; set; }
        public RelayCommand ActionEditWidgets { get; set; }
        public RelayCommand ActionSave { get; set; }
        public RelayCommand ActionDelete { get; set; }
        #endregion

        #region Variables
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IFaceService _faceService;
        private readonly IDispatcherService _dispatcher;
        private readonly RemoteService _remoteService;
        private readonly MiriotService _miriotService;
        private ObservableCollection<MiriotConfiguration> _configurations;
        private ObservableCollection<WidgetModel> _widgets;
        private User _user;
        private MiriotConfiguration _selectedConfiguration;
        private bool _hasNoConfiguration;
        private string MiriotId;
        #endregion

        public ObservableCollection<MiriotConfiguration> Configurations
        {
            get { return _configurations; }
            set { Set(ref _configurations, value); }
        }

        public User User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public MiriotConfiguration SelectedConfiguration
        {
            get { return _selectedConfiguration; }
            set
            {
                Set(ref _selectedConfiguration, value);
                if (value != null)
                    OnSelectionChanged();
            }
        }

        public bool HasNoConfiguration
        {
            get { return _hasNoConfiguration; }
            set { Set(ref _hasNoConfiguration, value); }
        }

        public ObservableCollection<WidgetModel> Widgets
        {
            get { return _widgets; }
            set { Set(() => Widgets, ref _widgets, value); }
        }

        public ProfileViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            IDispatcherService dispatcher,
            RemoteService remoteService,
            MiriotService miriotService) : base(navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _dispatcher = dispatcher;
            _remoteService = remoteService;
            _miriotService = miriotService;
            ActionEditName = new RelayCommand(OnEditName);
            ActionEditWidgets = new RelayCommand(OnEditWidgets);
            ActionSave = new RelayCommand(OnSave);
            ActionDelete = new RelayCommand(async () => await OnDelete());

            Configurations = new ObservableCollection<MiriotConfiguration>();
            Widgets = new ObservableCollection<WidgetModel>();
        }

        private void OnEditName()
        {
            if (SelectedConfiguration == null)
                return;

            RaisePropertyChanged(() => SelectedConfiguration);
            RaisePropertyChanged(() => SelectedConfiguration.Name);
            HasNoConfiguration = false;
        }

        private void UpdateWidgets()
        {
            foreach (var w in Widgets)
            {
                AddRemoveWidget(w);
            }
        }

        private async void OnEditWidgets()
        {
            UpdateWidgets();

            await RefreshUserAsync();
        }

        private void AddRemoveWidget(WidgetModel w)
        {
            // If desactivated
            if (!w.IsActive)
            {
                var ww = SelectedConfiguration.Widgets.FirstOrDefault(e => e.Type == w.Type);

                // Should be existing
                if (ww != null)
                {
                    // Remove
                    SelectedConfiguration.Widgets.Remove(ww);
                }
            }
            else // if activated
            {
                var ww = SelectedConfiguration.Widgets.FirstOrDefault(e => e.Type == w.Type);

                // Should not be existing
                if (ww == null)
                    SelectedConfiguration.Widgets.Add(w.ToWidget());
                else
                {
                    var newW = w.ToWidget();
                    ww.Infos = newW.Infos;
                    ww.X = newW.X;
                    ww.Y = newW.Y;
                }
            }
        }

        private async Task OnDelete()
        {
            var isSuccess = false; // await _faceService.DeletePerson(User.Id);

            if (isSuccess)
                _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Utilisateur supprimé", "Information"));
            else
                _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Erreur lors de la suppression. Réessayez.", "Alerte"));
        }

        private void OnSave()
        {
            UpdateWidgets();

            Task.Run(async () =>
            {
                var isSuccess = await UpdateUserAsync();

                if (isSuccess)
                    _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Modifications sauvegardées", "Information"));
                else
                    _dispatcher.Invoke(async () => await _dialogService.ShowMessage("Erreur lors de la sauvegarde. Réessayez.", "Alerte"));
            });
        }

        private Task<bool> RefreshUserAsync()
        {
            return _remoteService.SendAsync(new RemoteParameter() { Command = RemoteCommands.LoadUser });
        }

        private async Task<bool> UpdateUserAsync()
        {
            await _miriotService.UpdateConfigurationAsync(SelectedConfiguration);
            return await RefreshUserAsync();
        }

        public void SetParameters(MiriotParameter parameter)
        {
            User = parameter.User;
            MiriotId = parameter.Id;
        }

        private async void OnSelectionChanged()
        {
            if (MiriotId == SelectedConfiguration.MiriotDeviceId)
                _remoteService.Command(RemoteCommands.MiriotConfiguring);

            await LoadWidgetsAsync();
        }

        protected override async Task InitializeAsync()
        {
            if (User == null)
                return;

            HasNoConfiguration = false;
            Configurations.Clear();

            if (User.Devices == null)
                User.Devices = new List<MiriotConfiguration>();

            if (!User.Devices.Any())
            {
                var config = new MiriotConfiguration() { MiriotDeviceId = MiriotId, Name = "Miriot" };
                User.Devices.Add(config);
                HasNoConfiguration = true;
            }

            foreach (var d in User.Devices)
                Configurations.Add(d);

            SelectedConfiguration = Configurations.FirstOrDefault(e => e.MiriotDeviceId == MiriotId);

            if (SelectedConfiguration == null)
            {
                HasNoConfiguration = true;
                var config = new MiriotConfiguration()
                {
                    MiriotDeviceId = MiriotId,
                    Name = "Miriot",
                    UserId = User.Id
                };
                User.Devices.Add(config);
                SelectedConfiguration = await _miriotService.CreateConfiguration(config);
            }
        }

        private async Task LoadWidgetsAsync()
        {
            Widgets.Clear();

            var widgets = new List<WidgetModel>();

            foreach (var type in Enum.GetValues(typeof(WidgetType)))
            {
                var wt = (WidgetType)type;

                var widgetEntity = SelectedConfiguration.Widgets.FirstOrDefault(e => e.Type == wt);

                var widgetModel = wt.ToModel(widgetEntity);

                if (widgetModel.IsBuiltIn)
                    continue;

                if (widgetEntity != null)
                {
                    await widgetModel.Load();
                    widgetModel.SetActive();
                }
                else
                {
                    widgetModel.IsActive = false;
                }

                widgets.Add(widgetModel);
            }

            Widgets = new ObservableCollection<WidgetModel>(widgets.OrderBy(e => e.Title));
        }
    }
}
