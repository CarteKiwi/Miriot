using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Model;
using Miriot.Services;
using System;
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
            RemoteService remoteService) : base(navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _dispatcher = dispatcher;
            _remoteService = remoteService;

            ActionEditName = new RelayCommand(OnEditName);
            ActionEditWidgets = new RelayCommand(OnEditWidgets);
            ActionSave = new RelayCommand(OnSave);
            ActionDelete = new RelayCommand(async () => await OnDelete());

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

        private void OnEditWidgets()
        {
            throw new NotImplementedException();
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
            await _faceService.UpdateUserDataAsync(User);
            var user = await _remoteService.CommandAsync<User>(RemoteCommands.UpdateUser);
            return false; //await _faceService.UpdatePerson(User, User.Picture);
        }

        public void SetParameters(MiriotParameter parameter)
        {
            User = parameter.User;
            MiriotId = parameter.Id;
        }

        private async void OnSelectionChanged()
        {
            await LoadWidgetsAsync();
        }

        protected override async Task InitializeAsync()
        {
            if (User == null)
                return;

            if (User.UserData.Devices == null)
                User.UserData.Devices = new System.Collections.Generic.List<MiriotConfiguration>();

            Configurations = new ObservableCollection<MiriotConfiguration>(User.UserData.Devices);

            SelectedConfiguration = User.UserData.Devices.FirstOrDefault(e => e.Id == MiriotId);

            if (SelectedConfiguration == null)
            {
                var config = new MiriotConfiguration(MiriotId, "Miriot");
                Configurations.Add(config);
                SelectedConfiguration = config;
                HasNoConfiguration = true;
            }
            else
            {
            }
        }

        private async Task LoadWidgetsAsync()
        {
            Widgets.Clear();

            foreach (var type in Enum.GetValues(typeof(WidgetType)))
            {
                var wt = (WidgetType)type;

                var widgetEntity = SelectedConfiguration.Widgets.FirstOrDefault(e => e.Type == wt);

                var widgetModel = wt.ToModel(widgetEntity);

                if (widgetEntity != null)
                {
                    await widgetModel.Load();
                    widgetModel.SetActive();
                }
                else
                {
                    widgetModel.IsActive = false;
                }

                Widgets.Add(widgetModel);
            }
        }
    }
}
