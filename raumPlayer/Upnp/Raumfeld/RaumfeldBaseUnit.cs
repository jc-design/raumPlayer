using JCDesign.Helper;
using JCDesign.Services;
using raumPlayer.Models;
using raumPlayer.ViewModels;
using raumPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Upnp.Enum;
using Upnp.Helper;
using Upnp.Raumfeld;
using Windows.Data.Xml.Dom;
using Windows.Devices.Enumeration;
using Windows.Web.Http;

namespace Upnp.Devices
{
    public class RaumfeldBaseUnit : IDisposable
    {
        #region IDisposable

        public async Task UnSubscribeDevices()
        {
            foreach (var device in Devices)
            {
                await device.Value.UnSubscribe();
            }
        }

        public async void Dispose()
        {
            await dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private async Task dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (disposed) return;
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                Devices.Clear();
                Devices = null;

                instance = null;
                // Dispose managed resources.
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            // Note disposing has been done.
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~RaumfeldBaseUnit()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Task.Run(() => dispose(false)).Wait();
        }

        #endregion

        #region Private Properties

        private bool disposed;
        //private DeviceInformation server = null;

        private raumfeldZoneConfig raumfeldZoneConfig = null;
        private string raumfeldGetZonesUpdateId = string.Empty;

        #endregion

        #region Public Properties

        private Dictionary<string, MediaDevice> devices = null;
        public Dictionary<string, MediaDevice> Devices
        {
            get { if (devices == null) { devices = new Dictionary<string, MediaDevice>(); } return devices; }
            set { devices = value; }
        }

        private MediaServer mediaServer = null;
        public MediaServer MediaServer
        {
            get { return mediaServer; }
            set { mediaServer = value; }
        }

        //private Dictionary<RaumfeldZone, MediaRenderer> zoneRenderers = null;
        //public Dictionary<RaumfeldZone, MediaRenderer> ZoneRenderers
        //{
        //    get { if (zoneRenderers == null) { zoneRenderers = new Dictionary<RaumfeldZone, MediaRenderer>(); } return zoneRenderers; }
        //    set { zoneRenderers = value; }
        //}

        //private Dictionary<string, MediaRenderer> roomRenderers = null;
        //public Dictionary<string, MediaRenderer> RoomRenderers
        //{
        //    get { if (roomRenderers == null) { roomRenderers = new Dictionary<string, MediaRenderer>(); } return roomRenderers; }
        //    set { roomRenderers = value; }
        //}

        //public string IPAddress => (((string[])server.Properties["System.Devices.IpAddress"])?.FirstOrDefault() ?? string.Empty);

        #endregion

        #region Constructor

        private RaumfeldBaseUnit() {}

        private static volatile RaumfeldBaseUnit instance;
        public static RaumfeldBaseUnit Instance
        {
            get
            {
                if (instance != null) return instance;
                lock (typeof(RaumfeldBaseUnit))
                {
                    if (instance == null)
                    {
                        instance = new RaumfeldBaseUnit();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs<ObservableCollection<ZoneViewModel>>> ZoneLoaded;

        public async void OnDeviceAdded(object sender, EventArgs<DeviceInformation> e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                // Examples of e.Value.Properties
                // System.Devices.ContainerId: e0acfce3-b225-4b5d-80d5-c240553ff32e
                // ([]) System.Devices.CompatibleIds: urn:schemas-upnp-org:device:MediaRenderer:1
                // ([]) System.Devices.IpAddress: 192.168.0.18
                // {A45C254E-DF1C-4EFD-8020-67D146A850E0},15: http://192.168.0.18:54797/e0acfce3-b225-4b5d-80d5-c240553ff32e.xml

                Guid ContainerId;
                string[] CompatibleIds = null;
                string[] IpAddress = null;
                string DeviceLocationInfo = null;

                foreach (var prop in e.Value.Properties)
                {
                    switch (prop.Key)
                    {
                        case "System.Devices.ContainerId":
                            ContainerId = (Guid)prop.Value;
                            break;
                        case "System.Devices.CompatibleIds":
                            CompatibleIds = prop.Value as string[];
                            break;
                        case "System.Devices.IpAddress":
                            IpAddress = prop.Value as string[];
                            break;
                        case "{A45C254E-DF1C-4EFD-8020-67D146A850E0} 15":
                            DeviceLocationInfo = prop.Value as string;
                            break;
                        default:
                            break;
                    }
                }

                //Check if Deive is a server or a renderer
                if ((CompatibleIds?.Count() ?? 0) > 0 && !string.IsNullOrEmpty(DeviceLocationInfo))
                {
                    MediaDeviceType mediaDeviceType;

                    switch (CompatibleIds[0])
                    {
                        case "urn:schemas-upnp-org:device:MediaServer:1":
                            mediaDeviceType = MediaDeviceType.MediaServer;
                            break;
                        case "urn:schemas-upnp-org:device:MediaRenderer:1":
                            mediaDeviceType = MediaDeviceType.MediaRenderer;
                            break;
                        default:
                            mediaDeviceType = MediaDeviceType.Unknown;
                            break;
                    }

                    MediaDevice mediaDevice = await loadMediaDeviceAsync(DeviceLocationInfo, mediaDeviceType);
                    Devices.Add(e.Value.Id, mediaDevice);

                    if (mediaDeviceType == MediaDeviceType.MediaServer)
                    {
                        //Assign MediaServer
                        MediaServer = mediaDevice as MediaServer;
                        MediaServer.SystemUpdateID_Changed += Shell.Instance.GetTuneInMenuItem().OnSystemUpdateID_Changed;
                    }

                   //await getRaumfeldZones();
                }
            });
        }

        public void OnDeviceRemoved(object sender, EventArgs<DeviceInformationUpdate> e)
        {
            if (Devices.TryGetValue(e.Value.Id, out MediaDevice device))
            {
                switch (device)
                {
                    case MediaServer mediaServer:

                        mediaServer.SystemUpdateID_Changed -= Shell.Instance.GetTuneInMenuItem().OnSystemUpdateID_Changed;
                        mediaServer.Dispose();
                        MediaServer = null;

                        //Start Waiting for new Server!!!
                        break;
                    case MediaRenderer mediaRenderer:
                        mediaRenderer.Dispose();
                        //await getRaumfeldZones();

                        break;
                    default:
                        break;
                }

                Devices.Remove(e.Value.Id);
            }
        }

        public void OnDeviceUpdated(object sender, EventArgs<DeviceInformationUpdate> e)
        {
            // Add logic here
            // What to do, when Devices are updated

            string update = string.Empty;
            foreach (var prop in e.Value.Properties)
            {
                update = update + prop.Key;
            }
        }

        #endregion

        #region Private Methods

        /// Wird nicht benötigt, da über UWP die benötigen Daten empfangen werden
        /// <summary>
        /// Get List of all devices, with uuid, ports and info .xml
        /// http://www.hifi-forum.de/index.php?action=browseT&forum_id=212&thread=420&postID=220#220
        /// </summary>
        /// <returns></returns>
        //private async Task raumfeldListDevices()
        //{
        //    if (server == null) { return; }

        //    string ipAddress = ((string[])server.Properties["System.Devices.IpAddress"]).FirstOrDefault();
        //    if (string.IsNullOrEmpty(ipAddress)) { return; }

        //    //Logger.GetLogger().LogChannel.LogMessage(string.Format("RaumfeldListDevices Uri: {0}", HttpStringHelper.CompleteHttpString(ipAddress, RaumFeldDefinitions.PORT_WEBSERVICE, RaumFeldDefinitions.LISTDEVICES)));

        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            using (HttpRequestMessage request = new HttpRequestMessage() { RequestUri = new Uri(HttpStringHelper.CompleteHttpString(ipAddress, RaumFeldDefinitions.PORT_WEBSERVICE, RaumFeldDefinitions.LISTDEVICES)), Method = HttpMethod.Get })
        //            {
        //                //if (!string.IsNullOrEmpty(raumfeldListDevicesUpdateId)) { request.Headers.Add("updateID", raumfeldListDevicesUpdateId); }
        //                using (HttpResponseMessage response = await client.SendRequestAsync(request))
        //                {
        //                    if (response.StatusCode == HttpStatusCode.Ok)
        //                    {
        //                        //raumfeldListDevicesUpdateId = response.Headers["updateID"];
        //                        string xmlResponse = await response.Content.ReadAsStringAsync();
        //                        raumfeldDevices = xmlResponse.Deserialize<RaumfeldDevices>();

        //                        //How To Handle ServerChanges
        //                        //RaumfeldFoundDevices?.Invoke(this, null);

        //                        //Kein Longpoll bei den Devices
        //                        //Die Liste ändert sich nicht korrket
        //                        //await raumfeldListDevices();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        //await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "RaumfeldListDevices");
        //        //Logger.GetLogger().SaveMessage(exception, "RaumfeldListDevices");
        //    }
        //}

        /// <summary>
        /// Check for RaumfeldZones
        /// http://www.hifi-forum.de/index.php?action=browseT&forum_id=212&thread=420&postID=220#220
        /// http://www.hifi-forum.de/index.php?action=browseT&forum_id=212&thread=420&postID=271#271
        /// Add CancelationTokkens if necessary
        /// </summary>
        /// <returns></returns>
        private async Task raumfeldGetZones()
        {
            if (MediaServer == null) { return; }

            string ipAddress = MediaServer.IpAddress;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage() { RequestUri = new Uri(HttpStringHelper.CompleteHttpString(ipAddress, RaumFeldDefinitions.PORT_WEBSERVICE, RaumFeldDefinitions.GETZONES)), Method = HttpMethod.Get })
                    {
                        if (!string.IsNullOrEmpty(raumfeldGetZonesUpdateId)) { request.Headers.Add("updateID", raumfeldGetZonesUpdateId); }
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (response.StatusCode == HttpStatusCode.Ok)
                            {
                                raumfeldGetZonesUpdateId = response.Headers["updateID"];
                                string xmlResponse = await response.Content.ReadAsStringAsync();
                                raumfeldZoneConfig = XmlHelper.Deserialize<raumfeldZoneConfig>(xmlResponse);

                                await matchRaumfeldZones();

                                await raumfeldGetZones();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "RaumfeldGetZones");
                Logger.GetLogger().SaveMessage(exception, "RaumfeldGetZones");
            }
        }

        /// <summary>
        /// Get Raumfeld DeviceDescription and create new MediaServer
        /// </summary>
        /// <returns></returns>
        //private async Task<MediaServer> loadMediaServerAsync()
        //{
        //    //if ((raumfeldDevices?.Devices?.Count() ?? 0) == 0 || server == null) { return null; }

        //    string uuid = ((Guid)server.Properties["System.Devices.ContainerId"]).ToString().ToLower();
        //    try
        //    {
        //        string serverDescription = raumfeldDevices.Devices.Where(server => server.Udn.Replace("uuid:", "").ToLower() == uuid).Select(server => server.Location).FirstOrDefault();

        //        if (!string.IsNullOrEmpty(serverDescription))
        //        {
        //            Uri serverUri = new Uri(serverDescription);
        //            var httpFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
        //            httpFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.NoCache;
        //            using (HttpClient client = new HttpClient())
        //            {
        //                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serverUri))
        //                {
        //                    request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
        //                    request.Headers.Add("Accept-Language", "en");
        //                    request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

        //                    using (HttpResponseMessage response = await client.SendRequestAsync(request))
        //                    {
        //                        if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
        //                        {
        //                            string xmlString = await response.Content.ReadAsStringAsync();

        //                            XmlDocument xmlDocument = new XmlDocument();
        //                            xmlDocument.LoadXml(xmlString);
        //                            if (xmlDocument != null)
        //                            {
        //                                DeviceDescription deviceDescription = xmlDocument.GetXml().Deserialize<DeviceDescription>();
        //                                MediaServer mediaServer = new MediaServer(deviceDescription, serverUri.Host, serverUri.Port)
        //                                {
        //                                    DeviceType = MediaDeviceType.MediaServer
        //                                };

        //                                await mediaServer.LoadServicesAsync();

        //                                return mediaServer;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "LoadMediaServer");
        //        Logger.GetLogger().SaveMessage(exception, "LoadMediaServerAsync");
        //    }
        //    return null;
        //}

        /// <summary>
        /// Get Raumfeld DeviceDescription and create new MediaRenderer
        /// </summary>
        /// <returns></returns>
        //private async Task<MediaRenderer> loadMediaRendererAsync(string rendererDescription, MediaDeviceType mediaDeviceType)
        //{
        //    //if ((raumfeldDevices?.Devices?.Count() ?? 0) == 0 || server == null) { return null; }

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(rendererDescription))
        //        {
        //            Uri serverUri = new Uri(rendererDescription);
        //            var httpFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
        //            httpFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.NoCache;
        //            using (HttpClient client = new HttpClient(httpFilter))
        //            {
        //                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serverUri))
        //                {
        //                    request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
        //                    request.Headers.Add("Accept-Language", "en");
        //                    request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

        //                    using (HttpResponseMessage response = await client.SendRequestAsync(request))
        //                    {
        //                        if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
        //                        {
        //                            string xmlString = await response.Content.ReadAsStringAsync();

        //                            XmlDocument xmlDocument = new XmlDocument();
        //                            xmlDocument.LoadXml(xmlString);
        //                            if (xmlDocument != null)
        //                            {
        //                                DeviceDescription deviceDescription = xmlDocument.GetXml().Deserialize<DeviceDescription>();

        //                                MediaRenderer mediaRenderer = new MediaRenderer(deviceDescription, serverUri.Host, serverUri.Port);
        //                                await mediaRenderer.LoadServicesAsync();

        //                                mediaRenderer.DeviceType = mediaDeviceType;

        //                                return mediaRenderer;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "LoadMediaRenderer");
        //        Logger.GetLogger().SaveMessage(exception, "LoadMediaRendererAsync");
        //    }

        //    return null;
        //}

        /// <summary>
        /// Get Raumfeld DeviceDescription and create new MediaRenderer
        /// </summary>
        /// <returns></returns>
        private async Task<MediaDevice> loadMediaDeviceAsync(string rendererDescription, MediaDeviceType mediaDeviceType)
        {
            try
            {
                if (!string.IsNullOrEmpty(rendererDescription))
                {
                    Uri serverUri = new Uri(rendererDescription);
                    var httpFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                    httpFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.NoCache;
                    using (HttpClient client = new HttpClient(httpFilter))
                    {
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serverUri))
                        {
                            request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                            request.Headers.Add("Accept-Language", "en");
                            request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

                            using (HttpResponseMessage response = await client.SendRequestAsync(request))
                            {
                                if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                                {
                                    string xmlString = await response.Content.ReadAsStringAsync();

                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(xmlString);
                                    if (xmlDocument != null)
                                    {
                                        DeviceDescription deviceDescription = xmlDocument.GetXml().Deserialize<DeviceDescription>();

                                        if (mediaDeviceType == MediaDeviceType.MediaServer)
                                        {
                                            MediaServer mediaServer = new MediaServer(deviceDescription, serverUri.Host, serverUri.Port)
                                            {
                                                DeviceType = mediaDeviceType
                                            };
                                            await mediaServer.LoadServicesAsync();

                                            return mediaServer;
                                        }
                                        else
                                        {
                                            MediaRenderer mediaRenderer = new MediaRenderer(deviceDescription, serverUri.Host, serverUri.Port)
                                            {
                                                DeviceType = mediaDeviceType
                                            };
                                            await mediaRenderer.LoadServicesAsync();

                                            mediaRenderer.DeviceType = mediaDeviceType;

                                            return mediaRenderer;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "loadMediaDeviceAsync");
                Logger.GetLogger().SaveMessage(exception, "loadMediaDeviceAsync");
            }

            return null;
        }


        private async Task matchRaumfeldZones()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                ObservableCollection<ZoneViewModel> zoneViewModels = null;
                bool notLoaded = false;

                do
                {
                    zoneViewModels = new ObservableCollection<ZoneViewModel>();
                    notLoaded = false;

                    if (MediaServer == null) { notLoaded = true; }
                    else
                    {
                        foreach (var zone in raumfeldZoneConfig.ZoneList)
                        {
                            ZoneViewModel zoneViewModel = null;

                            // Get device from ZoneConfig
                            // Check if Device is already loaded
                            // If not break and wait
                            if (Devices.Where(d => d.Value.UDN == zone.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer zoneRenderer)
                            {
                                zoneViewModel = new ZoneViewModel(Shell.Instance, MediaServer, zoneRenderer);
                            }
                            else
                            {
                                notLoaded = true;
                                break;
                            }

                            foreach (var room in zone.RoomList)
                            {
                                // Check if Device is already loaded
                                // If not break and wait
                                if (Devices.Where(d => d.Value.UDN == room.Renderer.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer roomRenderer)
                                {
                                    if (zoneViewModel != null)
                                    {
                                        zoneViewModel.Rooms.Add(new Room(zoneViewModel, roomRenderer) { Name = room.Name, PowerState = room.PowerState, Udn = room.Udn });
                                    }
                                }
                                else
                                {
                                    notLoaded = true;
                                    break;
                                }
                            }

                            if (zoneViewModel != null)
                            {
                                await zoneViewModel.InitZone();
                                zoneViewModels.Add(zoneViewModel);
                            }
                        }

                        if ((raumfeldZoneConfig?.UnAssignedRooms?.Count() ?? 0) > 0)
                        {
                            ZoneViewModel zoneViewModel = new ZoneViewModel(Shell.Instance, null, null);

                            foreach (var room in raumfeldZoneConfig.UnAssignedRooms)
                            {
                                // Check if Device is already loaded
                                // If not break and wait
                                if (Devices.Where(d => d.Value.UDN == room.Renderer.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer roomRenderer)
                                {
                                    if (zoneViewModel != null)
                                    {
                                        zoneViewModel.Rooms.Add(new Room(zoneViewModel, roomRenderer) { Name = room.Name, PowerState = room.PowerState, Udn = room.Udn });
                                    }
                                }
                                else
                                {
                                    notLoaded = true;
                                    break;
                                }

                                zoneViewModels.Add(zoneViewModel);
                            }
                        }
                    }

                    if (notLoaded) { await Task.Delay(TimeSpan.FromMilliseconds(250)); }

                } while (notLoaded);


                // Check if all devices were loaded
                // Take into account, that the server is in "Devices" as well
                // RaumfeldZoneConfig stores only MediaRenderer
                //if (Devices.Count()-1 < count) { return; }

                ZoneLoaded?.Invoke(this, new EventArgs<ObservableCollection<ZoneViewModel>>(zoneViewModels));
            });
        }

        //private async Task<ObservableCollection<ZoneViewModel>> loadZones(bool listDevices)
        //{
        //    if (listDevices)
        //    {
        //        await Task.Delay(TimeSpan.FromMilliseconds(500));
        //        await raumfeldListDevices();
        //    }

        //    var zones = raumfeldZoneConfig.ZoneList.Select(z => z).ToList();

        //    foreach (var item in raumfeldDevices.Devices)
        //    {
        //        Debug.WriteLine("raumfeldDevices" + item.Name + item.Udn);
        //    }

        //    foreach (var item in raumfeldZoneConfig.ZoneList)
        //    {
        //        Debug.WriteLine("RaumfeldZoneConfig" + item.Udn);
        //    }


        //    List<RaumfeldRoom> rooms = new List<RaumfeldRoom>();
        //    rooms.AddRange(raumfeldZoneConfig.ZoneList.SelectMany(z => z.RoomList).ToList());
        //    rooms.AddRange(raumfeldZoneConfig.UnAssignedRooms.Select(r => r).ToList());

        //    foreach (var mediarenderer in ZoneRenderers.Values)
        //    {
        //        var renderer = zones.Where(z => z.Udn == mediarenderer.UDN).FirstOrDefault();
        //        if (renderer != null) { await mediarenderer.UnSubscribe(); }
        //    }

        //    foreach (var roomrenderer in RoomRenderers.Values)
        //    {
        //        var renderer = rooms.Where(z => z.Udn == roomrenderer.UDN).FirstOrDefault();
        //        if (renderer != null) { await roomrenderer.UnSubscribe(); }
        //    }

        //    ZoneRenderers = new Dictionary<RaumfeldZone, MediaRenderer>();
        //    RoomRenderers = new Dictionary<string, MediaRenderer>();

        //    foreach (var device in raumfeldDevices.Devices)
        //    {
        //        RaumfeldZone zone = zones.Where(z => z.Udn == device.Udn).FirstOrDefault();
        //        if (zone != null)
        //        {
        //            Debug.WriteLine("Loop"+zone.Udn);
        //            ZoneRenderers[zone] = await loadMediaRendererAsync(device.Location, MediaDeviceType.MediaRenderer);
        //        }
        //        RaumfeldRoom room = rooms.Where(r => r.Renderer.Udn == device.Udn).FirstOrDefault();
        //        if (room != null)
        //        {
        //            RoomRenderers.Add(device.Udn, await loadMediaRendererAsync(device.Location, MediaDeviceType.RoomRenderer));
        //        }
        //    }

        //    var zoneViewModels = new ObservableCollection<ZoneViewModel>();

        //    foreach (var renderer in ZoneRenderers)
        //    {
        //        ZoneViewModel zoneViewModel = new ZoneViewModel(Shell.Instance, MediaServer, renderer.Value);

        //        Debug.WriteLine("Loop" + renderer.Value.Name);

        //        foreach (RaumfeldRoom room in renderer.Key.RoomList)
        //        {
        //            MediaRenderer roomRenderer = RoomRenderers.Where(z => z.Key == room.Renderer.Udn).Select(z => z.Value).FirstOrDefault();
        //            if (roomRenderer != null)
        //            {
        //                zoneViewModel.Rooms.Add(new Room(zoneViewModel, roomRenderer) { Name = room.Name, PowerState = room.PowerState, Udn = room.Udn });
        //            }
        //        }

        //        await zoneViewModel.InitZone();
        //        zoneViewModels.Add(zoneViewModel);
        //    }

        //    //Add UnAssignedRooms
        //    if ((raumfeldZoneConfig?.UnAssignedRooms?.Count() ?? 0 ) > 0)
        //    {
        //        ZoneViewModel zoneViewModel = new ZoneViewModel(Shell.Instance, null, null);

        //        foreach (RaumfeldRoom room in raumfeldZoneConfig.UnAssignedRooms)
        //        {
        //            MediaRenderer roomRenderer = RoomRenderers.Where(z => z.Key == room.Renderer.Udn).Select(z => z.Value).FirstOrDefault();
        //            if (roomRenderer != null)
        //            {
        //                zoneViewModel.Rooms.Add(new Room(zoneViewModel, roomRenderer) { Name = room.Name, PowerState = room.PowerState, Udn = room.Udn });
        //            }
        //        }
        //        zoneViewModels.Add(zoneViewModel);
        //    }

        //    return zoneViewModels;
        //}

        #endregion

        #region Public Methods

        public string CreateNewGuid()
        {
            string newGuid = string.Empty;

            //Loop as long as randomly created guid is not existing in Zones
            do
            {
                newGuid = string.Format("uuid:{0}", Guid.NewGuid());
            } while (Devices.Where(d => d.Value.UDN == newGuid).Count() != 0);
            return newGuid;
        }

        public async Task<bool> ConnectRoomToZone(string roomUdn, string zoneUdn)
        {
            return await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                string ipAddress = MediaServer.IpAddress;
                bool returnValue = false;

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string uri = string.Format("http://{0}:{1}/connectRoomToZone?zoneUDN={2}&roomUDN={3}", ipAddress, RaumFeldDefinitions.PORT_WEBSERVICE, zoneUdn, roomUdn);
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
                        {
                            request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                            request.Headers.Add("Accept-Language", "en");
                            request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

                            using (HttpResponseMessage response = await client.SendRequestAsync(request))
                            {
                                returnValue = true;
                                //Do some stuff?
                                //Debug.WriteLine(await response.Content.ReadAsStringAsync());
                            }
                        }
                    }
                }

                catch (Exception exception)
                {
                    await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), exception.HResult, exception.Message), "ConnectRoomToZone");
                    Logger.GetLogger().SaveMessage(exception, "ConnectRoomToZone");
                }

                return returnValue;
            });
        }

        public async Task<bool> InitializeBaseUnit()
        {
            Devices = new Dictionary<string, MediaDevice>();

            do
            {
                if (MediaServer != null /*&& (raumfeldDevices?.Devices?.Count() ?? 0) > 0 && (RaumfeldZoneConfig?.ZoneList?.Count() ?? 0) > 0*/)
                {
                    //await raumfeldListDevices();

                    //if (MediaServer != null) { MediaServer.SystemUpdateID_Changed -= Shell.Instance.GetTuneInMenuItem().OnSystemUpdateID_Changed; }
                    //MediaServer = await loadMediaServerAsync();
                    //if (MediaServer != null)
                    //{
                    //    MediaServer.SystemUpdateID_Changed += Shell.Instance.GetTuneInMenuItem().OnSystemUpdateID_Changed;
                    //    break;
                    //}
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250));

            } while (MediaServer == null /*|| (raumfeldDevices?.Devices?.Count() ?? 0) == 0 || (RaumfeldZoneConfig?.ZoneList?.Count() ?? 0) == 0*/);

            raumfeldGetZones();

            //var zones = await loadZones(false);
            //ZoneLoaded?.Invoke(this, new EventArgs<ObservableCollection<ZoneViewModel>>(zones));

            return (MediaServer != null /*&& (raumfeldDevices?.Devices?.Count() ?? 0) > 0 && (RaumfeldZoneConfig?.ZoneList?.Count() ?? 0) > 0*/);
        }

        #endregion
    }
}
