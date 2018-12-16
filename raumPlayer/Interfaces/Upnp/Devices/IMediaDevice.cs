using Prism.Events;
using raumPlayer;
using raumPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.System.Threading;
using Windows.Web.Http;

namespace Upnp
{
    public interface IMediaDevice
    {
        AVTransport AVTransport { get; set; }
        ConnectionManager ConnectionManager { get; set; }
        ContentDirectory ContentDirectory { get; set; }
        MediaReceiverRegistrar MediaReceiverRegistrar { get; set; }
        RenderingControl RenderingControl { get; set; }

        string Name { get; }
        string Udn { get; }
        string IpAddress { get; }

        MediaDeviceType DeviceType { get; set; }
        Dictionary<string, ServiceAction> ServiceActions { get; set; }

        Task InitializeAsync();
    }
}
