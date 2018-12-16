using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.System.Threading;

namespace raumPlayer.Interfaces
{
    public interface INetWorkDeviceWatcher
    {
        List<DeviceInformation> NetWorkDevices { get; set; }

        void StartDeviceWatcher();
        void StopDeviceWatcher();
        void RefreshDeviceWatcher();

    }
}
