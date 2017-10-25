using Miriot.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.System.RemoteSystems;

namespace Miriot.ViewModels
{
    public class ConnectViewModel : CustomViewModel
    {
        private RemoteSystemWatcher _remoteSystemWatcher;
        private List<RemoteSystem> _remoteSystems;

        public void Initialize()
        {
            // store filter list
            List<IRemoteSystemFilter> listOfFilters = MakeFilterList();

            // construct watcher with the list
            _remoteSystemWatcher = RemoteSystem.CreateWatcher(listOfFilters);

           // _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcherOnRemoteSystemAdded;

            _remoteSystemWatcher.Start();
        }

        private void RemoteSystemWatcherOnRemoteSystemAdded(RemoteSystemWatcher watcher, RemoteSystemAddedEventArgs args)
        {
            var system = args.RemoteSystem;

            _remoteSystems.Add(system);
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
            listOfTypes.Add(RemoteSystemKinds.Phone);

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
    }
}
