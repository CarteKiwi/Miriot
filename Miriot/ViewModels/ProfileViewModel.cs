using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Miriot.Model;

namespace Miriot.Core.ViewModels
{
    public class ProfileViewModel : CustomViewModel
    {
        #region Commands
        public RelayCommand<string> ActionEditName { get; set; }
        public RelayCommand ActionEditWidgets { get; set; }
        public RelayCommand ActionSave { get; set; }
        public RelayCommand ActionDelete { get; set; }
        #endregion

        #region Variables
        private readonly IDialogService _dialogService;
        private readonly IFaceService _faceService;
        private readonly IDispatcherService _dispatcher;
        private readonly RemoteService _remoteService;
        private ObservableCollection<MiriotConfiguration> _configurations;
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
            set { Set(ref _selectedConfiguration, value); }
        }

        public bool HasNoConfiguration
        {
            get { return _hasNoConfiguration; }
            set { Set(ref _hasNoConfiguration, value); }
        }

        public ProfileViewModel(
            IDialogService dialogService,
            IDispatcherService dispatcher,
            RemoteService remoteService)
        {
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            _remoteService = remoteService;

            ActionEditName = new RelayCommand<string>(OnEditName);
            ActionEditWidgets = new RelayCommand(OnEditWidgets);
            ActionSave = new RelayCommand(OnSave);
            ActionDelete = new RelayCommand(async () => await OnDelete());
        }

        private void OnEditName(string obj)
        {
            SelectedConfiguration.Name = obj;
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
            return false; //await _faceService.UpdatePerson(User, User.Picture);
        }

        public void SetParameters(MiriotParameter parameter)
        {
            User = parameter.User;
            MiriotId = parameter.Id;
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
    }
}
