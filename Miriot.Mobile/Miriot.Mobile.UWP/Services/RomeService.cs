using Miriot.Common.Model;
using Miriot.Model;
using Miriot.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;

namespace Miriot.Mobile.UWP.Services
{
    public class RomeService : IRomeService
    {
        private RemoteSystemWatcher _remoteSystemWatcher;
        private List<RomeRemoteSystem> _remoteSystems;
        private AppServiceConnection _appServiceConnection;

        public bool IsInitialized { get; set; }

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();

        public Action<RomeRemoteSystem> Added { get; set; }

        public async Task InitializeAsync()
        {
            // store filter list
            //List<IRemoteSystemFilter> listOfFilters = MakeFilterList();

            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();

            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                _remoteSystems = new List<RomeRemoteSystem>();

                // construct watcher with the list
                _remoteSystemWatcher = RemoteSystem.CreateWatcher();
                _remoteSystemWatcher.RemoteSystemAdded += OnRemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved += OnRemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated += OnRemoteSystemUpdated;
                _remoteSystemWatcher.Start();
            }
        }

        private void OnRemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            var system = _remoteSystems.First(x => x.Id == args.RemoteSystem.Id);
            _remoteSystems.Remove(system);
            _remoteSystems.Add(ToRomeRemoteSystem(args.RemoteSystem));
        }

        private void OnRemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            var system = _remoteSystems.FirstOrDefault(s => s.Id == args.RemoteSystemId);
            if (system != null)
            {
                _remoteSystems.Remove(system);
            }
        }

        private void OnRemoteSystemAdded(RemoteSystemWatcher watcher, RemoteSystemAddedEventArgs args)
        {
            var system = ToRomeRemoteSystem(args.RemoteSystem);

            _remoteSystems.Add(system);
            Added?.Invoke(system);
        }

        private static RomeRemoteSystem ToRomeRemoteSystem(RemoteSystem rs)
        {
            return new RomeRemoteSystem(rs)
            {
                DisplayName = rs.DisplayName,
                Id = rs.Id,
                Kind = rs.Kind.ToString(),
                Status = rs.Status.ToString()
            };
        }

        private List<IRemoteSystemFilter> MakeFilterList()
        {
            // construct an empty list
            List<IRemoteSystemFilter> localListOfFilters = new List<IRemoteSystemFilter>();

            // construct a discovery type filter that only allows "proximal" connections:
            RemoteSystemDiscoveryTypeFilter discoveryFilter = new RemoteSystemDiscoveryTypeFilter(RemoteSystemDiscoveryType.Proximal);

            // construct a device type filter that only allows desktop and mobile devices:
            // For this kind of filter, we must first create an IIterable of strings representing the device types to allow.
            // These strings are stored as static read-only properties of the RemoteSystemKinds class.
            List<String> listOfTypes = new List<String>();
            listOfTypes.Add(RemoteSystemKinds.Desktop);
            listOfTypes.Add(RemoteSystemKinds.Iot);

            // Put the list of device types into the constructor of the filter
            RemoteSystemKindFilter kindFilter = new RemoteSystemKindFilter(listOfTypes);

            // construct an availibility status filter that only allows devices marked as available:
            RemoteSystemStatusTypeFilter statusFilter = new RemoteSystemStatusTypeFilter(RemoteSystemStatusType.Available);

            // add the 3 filters to the listL
            localListOfFilters.Add(discoveryFilter);
            localListOfFilters.Add(kindFilter);
            localListOfFilters.Add(statusFilter);

            // return the list
            return localListOfFilters;
        }

        public Task<bool> RemoteLaunchUri(RomeRemoteSystem remoteSystem, Uri uri)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ConnectAsync(RomeRemoteSystem remoteSystem)
        {
            if (_appServiceConnection == null)
            {
                // Set up a new app service connection. The app service name and package family name that
                // are used here correspond to the AppServices UWP sample.
                _appServiceConnection = new AppServiceConnection
                {
                    AppServiceName = "com.gdm.miriot.agent",
                    PackageFamilyName = "Miriot_0yq8da09mhzv6"
                };

                // a valid RemoteSystem object is needed before going any further
                if (remoteSystem == null)
                {
                    return false;
                }

                // Create a remote system connection request for the given remote device
                RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest((RemoteSystem)remoteSystem.NativeObject);

                // "open" the AppServiceConnection using the remote request
                AppServiceConnectionStatus status = await _appServiceConnection.OpenRemoteAsync(connectionRequest);

                // only continue if the connection opened successfully
                if (status != AppServiceConnectionStatus.Success)
                {
                    _appServiceConnection = null;
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> SendCommandAsync(RomeRemoteSystem remoteSystem, string command)
        {
            if (await ConnectAsync(remoteSystem))
            {
                ValueSet inputs = new ValueSet();
                inputs.Add("Command", command);

                AppServiceResponse response = await _appServiceConnection.SendMessageAsync(inputs);

                string result = "";

                // check that the service successfully received and processed the message
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    // Get the data that the service returned:
                    result = response.Message["Result"] as string;
                }

                return true;
            }

            return false;
        }

        public async Task<User> GetRemoteUserAsync(RomeRemoteSystem remoteSystem)
        {
            if (await ConnectAsync(remoteSystem))
            {
                ValueSet inputs = new ValueSet();
                inputs.Add("Command", "GetUser");

                AppServiceResponse response = await _appServiceConnection.SendMessageAsync(inputs);

                // check that the service successfully received and processed the message
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    var res = response.Message["Result"];
                    return JsonConvert.DeserializeObject<User>(res.ToString());
                }
            }

            return null;
        }
    }
}
