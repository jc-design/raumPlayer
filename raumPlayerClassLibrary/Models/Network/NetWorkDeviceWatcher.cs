
using Prism.Events;
using raumPlayer.Interfaces;
using raumPlayer.PrismEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace raumPlayer.Models
{
    public class NetWorkDeviceWatcher : INetWorkDeviceWatcher
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;

        private string[] requestedProperties;
        private string aqsFilter;

        private DeviceWatcher watcher;

        public List<DeviceInformation> NetWorkDevices { get; set; } = new List<DeviceInformation>();

        public void StartDeviceWatcher()
        {
            watcher.Added += watcherDeviceAdded;
            watcher.Updated += watcherDeviceUpdated;
            watcher.Removed += watcherDeviceRemoved;
            watcher.EnumerationCompleted += watcherDeviceEnumCompleted;
            watcher.Stopped += watcherDeviceStopped;

            watcher.Start();
        }
        public void StopDeviceWatcher()
        {
            if (null != watcher)
            {
                // First unhook all event handlers except the stopped handler. This ensures our
                // event handlers don't get called after stop, as stop won't block for any "in flight" 
                // event handler calls.  We leave the stopped handler as it's guaranteed to only be called
                // once and we'll use it to know when the query is completely stopped. 
                watcher.Added -= watcherDeviceAdded;
                watcher.Updated -= watcherDeviceUpdated;
                watcher.Removed -= watcherDeviceRemoved;
                watcher.EnumerationCompleted -= watcherDeviceEnumCompleted;
                watcher.Stopped -= watcherDeviceStopped;

                if (DeviceWatcherStatus.Started == watcher.Status || DeviceWatcherStatus.EnumerationCompleted == watcher.Status)
                {
                    watcher.Stop();
                }
            }
        }
        public void RefreshDeviceWatcher()
        {
            StopDeviceWatcher();
            StartDeviceWatcher();
        }

        public NetWorkDeviceWatcher(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;

            // Request some additional properties.  In this saample, these extra properties are just used in the ResultsListViewTemplate. 
            // Take a look there in the XAML. Also look at the coverter used by the XAML GeneralPropertyValueConverter.  In general you just use
            // DeviceInformation.Properties["<property name>"] to get a property. e.g. DeviceInformation.Properties["System.Devices.InterfaceClassGuid"].
            requestedProperties = new string[] {
                "System.Devices.ContainerId",
                "System.Devices.CompatibleIds",
                "System.Devices.IpAddress",
                "{A45C254E-DF1C-4EFD-8020-67D146A850E0},15",        //"PKEY_Device_LocationInfo")
                };

            aqsFilter = "System.Devices.Manufacturer:~~\"Raumfeld\" OR System.Devices.Manufacturer:~~\"Teufel\"";

            watcher = DeviceInformation.CreateWatcher(aqsFilter, requestedProperties, DeviceInformationKind.AssociationEndpoint);
        }

        private void watcherDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            NetWorkDevices.Add(args);
            eventAggregator.GetEvent<NetWorkDeviceAddedEvent>().Publish(args);
        }

        private void watcherDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // Find the corresponding updated DeviceInformation in the collection and pass the update object
            // to the Update method of the existing DeviceInformation. This automatically updates the object
            // for us.
            // Update Servers
            var device = NetWorkDevices.Select(s => s).Where(s => s.Id == args.Id).FirstOrDefault();
            if (device != null)
            {
                eventAggregator.GetEvent<NetWorkDeviceUpdatedEvent>().Publish(device);
            }
        }

        private void watcherDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // Find the corresponding DeviceInformation in the collection and remove it
            var device = NetWorkDevices.Select(s => s).Where(s => s.Id == args.Id).FirstOrDefault();
            if (device != null)
            {
                NetWorkDevices.Remove(device);
                eventAggregator.GetEvent<NetWorkDeviceUpdatedEvent>().Publish(device);
            }
        }

        private void watcherDeviceEnumCompleted(DeviceWatcher sender, object args)
        {
        }

        private void watcherDeviceStopped(DeviceWatcher sender, object args)
        {
        }
    }
}
