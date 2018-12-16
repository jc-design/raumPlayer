using Prism.Events;
using raumPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace raumPlayer.PrismEvents
{
    public class NetWorkDeviceUpdatedEvent : PubSubEvent<DeviceInformation>
    {
    }
}
