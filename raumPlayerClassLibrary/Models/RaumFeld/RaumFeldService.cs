using HtmlAgilityPack;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.PrismEvents;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Upnp;
using Windows.Data.Xml.Dom;
using Windows.Devices.Enumeration;
using Windows.Web.Http;

namespace raumPlayer.Models
{
    public class RaumFeldService : IRaumFeldService
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        //private readonly IShellViewModel shellViewModel;

        private readonly SemaphoreSlim semaphoreLock;

        /// <summary>
        /// Key:   NetWorkDeviceWatcher.Id
        /// Value: IMediaDevice
        /// </summary>
        private Dictionary<string, IMediaDevice> devices = null;
        private MediaServer mediaServer = null;

        private CancellationTokenSource cancellationTokenSource;

        private RaumFeldZoneConfig raumFeldZoneConfig = null;
        private string raumfeldGetZonesUpdateId = string.Empty;

        public RaumFeldService(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            //shellViewModel = shellViewModelInstance;

            semaphoreLock = new SemaphoreSlim(1);

            devices = new Dictionary<string, IMediaDevice>();

            eventAggregator.GetEvent<NetWorkDeviceAddedEvent>().Subscribe(onDeviceAdded, ThreadOption.UIThread);
            eventAggregator.GetEvent<NetWorkDeviceUpdatedEvent>().Subscribe(onDeviceUpdated, ThreadOption.UIThread);
            eventAggregator.GetEvent<NetWorkDeviceRemovedEvent>().Subscribe(onDeviceRemoved, ThreadOption.UIThread);
        }

        private async void onDeviceAdded(DeviceInformation args)
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

            foreach (var prop in args.Properties)
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

            //Check if Device is a server or a renderer
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

                if (!devices.TryGetValue(args.Id, out IMediaDevice device))
                {
                    IMediaDevice mediaDevice = await loadMediaDeviceAsync(DeviceLocationInfo, mediaDeviceType);
                    devices.Add(args.Id, mediaDevice);

                    if (mediaDeviceType == MediaDeviceType.MediaServer)
                    {
                        //Assign MediaServer
                        mediaServer = mediaDevice as MediaServer;
                    }

                    //await getRaumfeldZones();
                }
            }
        }
        private void onDeviceUpdated(DeviceInformation args)
        {
            // Add logic here
            // What to do, when Devices are updated

            //string update = string.Empty;
            //foreach (var prop in args.Properties)
            //{
            //    update = update + prop.Key;
            //}
        }
        private void onDeviceRemoved(DeviceInformation args)
        {
            if (devices.TryGetValue(args.Id, out IMediaDevice device))
            {
                device = null;
                devices.Remove(args.Id);
            }
        }

        /// <summary>
        /// Creates a new instance of IMediaDevice
        /// </summary>
        /// <param name="rendererDescription">URI, where to fetch the MediaDevice desciptions</param>
        /// <param name="mediaDeviceType">MediaDevicetype, e.g. MediaServer</param>
        /// <returns></returns>
        private async Task<IMediaDevice> loadMediaDeviceAsync(string rendererDescription, MediaDeviceType mediaDeviceType)
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
                                        IMediaDevice device;

                                        if (mediaDeviceType == MediaDeviceType.MediaServer)
                                        {
                                            device = PrismUnityApplication.Current.Container.Resolve<MediaServer>(new ResolverOverride[]
                                                {
                                                   new ParameterOverride("deviceDescription", deviceDescription), new ParameterOverride("ipAddress", serverUri.Host), new ParameterOverride("port", serverUri.Port),
                                                });

                                            await device.InitializeAsync();

                                            return device;
                                        }
                                        else
                                        {
                                            device = PrismUnityApplication.Current.Container.Resolve<MediaRenderer>(new ResolverOverride[]
                                                {
                                                   new ParameterOverride("deviceDescription", deviceDescription), new ParameterOverride("ipAddress", serverUri.Host), new ParameterOverride("port", serverUri.Port), new ParameterOverride("deviceType", mediaDeviceType),
                                                });

                                            await device.InitializeAsync();

                                            return device;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();

                return null;
            }

            return null;
        }

        /// <summary>
        /// Check for RaumfeldZones
        /// http://www.hifi-forum.de/index.php?action=browseT&forum_id=212&thread=420&postID=220#220
        /// http://www.hifi-forum.de/index.php?action=browseT&forum_id=212&thread=420&postID=271#271
        /// Add CancelationTokkens if necessary
        /// </summary>
        /// <returns></returns>
        private async Task raumfeldGetZones()
        {
            if (mediaServer == null) { return; }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage() { RequestUri = new Uri(HtmlExtension.CompleteHttpString(mediaServer.IpAddress, RaumFeldStaticDefinitions.PORT_WEBSERVICE, RaumFeldStaticDefinitions.GETZONES)), Method = HttpMethod.Get })
                    {
                        if (!string.IsNullOrEmpty(raumfeldGetZonesUpdateId)) { request.Headers.Add("updateID", raumfeldGetZonesUpdateId); }
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (response.StatusCode == HttpStatusCode.Ok)
                            {
                                raumfeldGetZonesUpdateId = response.Headers["updateID"];
                                string xmlResponse = await response.Content.ReadAsStringAsync();
                                raumFeldZoneConfig = XmlExtension.Deserialize<RaumFeldZoneConfig>(xmlResponse);

                                await matchRaumfeldZones();

                                await raumfeldGetZones();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception();

                await messagingService.ShowErrorDialogAsync(exception);
            }
        }
        private async Task matchRaumfeldZones()
        {
            await semaphoreLock.WaitAsync();
            try
            {
                bool notLoaded = false;
                var zonesList = new List<ZoneViewModel>();

                do
                {
                    //shellViewModel.ZoneViewModels.Clear();
                    notLoaded = false;

                    if (mediaServer == null) { notLoaded = true; }
                    else
                    {
                        foreach (var zone in raumFeldZoneConfig.ZoneList)
                        {
                            ZoneViewModel zoneViewModel = null;

                            // Get device from ZoneConfig
                            // Check if Device is already loaded
                            // If not break and wait
                            if (devices.Where(d => d.Value.Udn == zone.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer zoneRenderer)
                            {
                                //zoneViewModel = new ZoneViewModel(Shell.Instance, MediaServer, zoneRenderer);
                                zoneViewModel = PrismUnityApplication.Current.Container.Resolve<ZoneViewModel>(new ResolverOverride[]
                                                {
                                                    new ParameterOverride("zoneViewModelRendererInstance", zoneRenderer),
                                                });
                            }
                            else
                            {
                                notLoaded = true;
                                break;
                            }

                            foreach (var room in zone.Rooms)
                            {
                                // Check if Device is already loaded
                                // If not break and wait
                                if (devices.Where(d => d.Value.Udn == room.Renderer.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer roomRenderer)
                                {
                                    if (zoneViewModel != null)
                                    {
                                        zoneViewModel.RoomViewModels.Add(PrismUnityApplication.Current.Container.Resolve<RoomViewModel>(new ResolverOverride[]
                                                                        {
                                                                            new ParameterOverride("zoneViewModelInstance", zoneViewModel), new ParameterOverride("roomRendererInstance", roomRenderer)
                                                                            , new ParameterOverride("name", room.Name), new ParameterOverride("udn", room.Udn), new ParameterOverride("powerState", room.PowerState)
                                                                        }));
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
                                //shellViewModel.ZoneViewModels.Add(zoneViewModel);
                                zonesList.Add(zoneViewModel);
                            }
                        }

                        if ((raumFeldZoneConfig?.UnAssignedRooms?.Count() ?? 0) > 0)
                        {
                            ZoneViewModel zoneViewModel = PrismUnityApplication.Current.Container.Resolve<ZoneViewModel>(new ResolverOverride[]
                                                {
                                                    new ParameterOverride("zoneViewModelRendererInstance", PrismUnityApplication.Current.Container.Resolve<MediaRenderer>(new ResolverOverride[]
                                                                                                            {
                                                                                                                new ParameterOverride("deviceDescription", new DeviceDescription()), new ParameterOverride("ipAddress", string.Empty), new ParameterOverride("port", 0), new ParameterOverride("deviceType", MediaDeviceType.Unknown),
                                                                                                            })),
                                                });

                            foreach (var room in raumFeldZoneConfig.UnAssignedRooms)
                            {
                                // Check if Device is already loaded
                                // If not break and wait
                                if (devices.Where(d => d.Value.Udn == room.Renderer.Udn).Select(d => d.Value).FirstOrDefault() is MediaRenderer roomRenderer)
                                {
                                    if (zoneViewModel != null)
                                    {
                                        zoneViewModel.RoomViewModels.Add(PrismUnityApplication.Current.Container.Resolve<RoomViewModel>(new ResolverOverride[]
                                                                        {
                                                                            new ParameterOverride("zoneViewModelInstance", zoneViewModel), new ParameterOverride("roomRendererInstance", roomRenderer)
                                                                            , new ParameterOverride("name", room.Name), new ParameterOverride("udn", room.Udn), new ParameterOverride("powerState", room.PowerState)
                                                                        }));
                                    }
                                }
                                else
                                {
                                    notLoaded = true;
                                    break;
                                }
                                //shellViewModel.ZoneViewModels.Add(zoneViewModel);
                                zonesList.Add(zoneViewModel);
                            }
                        }
                    }

                    if (notLoaded) { await Task.Delay(TimeSpan.FromMilliseconds(250)); }

                } while (notLoaded);

                eventAggregator.GetEvent<RaumFerldZonesLoadedEvent>().Publish(zonesList);
                //await shellViewModel.SetActiveZoneViewModel(string.Empty);
            }
            catch (Exception exception)
            {
                throw new Exception();

                await messagingService.ShowErrorDialogAsync(exception);
            }
            finally
            {
                semaphoreLock.Release();
            }
        }

        public string CreateNewGuid()
        {
            string newGuid = string.Empty;

            //Loop as long as randomly created guid is not existing in Zones
            do
            {
                newGuid = string.Format("uuid:{0}", Guid.NewGuid());
            } while (devices.Where(d => d.Value.Udn == newGuid).Count() != 0);
            return newGuid;
        }

        public async Task<bool> InitializeAsync()
        {
            devices = new Dictionary<string, IMediaDevice>();

            int count = 0;

            do
            {
                if (mediaServer != null)
                {
                    count++;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));

            } while (mediaServer == null || count < 50);

            Task t = raumfeldGetZones();

            return (mediaServer != null);
        }

        #region MediaServer

        /// <summary>
        /// Creates new CancellationTokenSource and invokes Cancel-Catch if browsing is active
        /// </summary>
        private async Task<CancellationToken> createNewToken()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            // Re-create the CancellationTokenSource.
            cancellationTokenSource = new CancellationTokenSource();

            await Task.Delay(TimeSpan.FromSeconds(1));

            return cancellationTokenSource.Token;
        }

        public async Task<bool> ConnectRoomToZone(string roomUdn, string zoneUdn)
        {
            if (mediaServer == null) { return false; }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = string.Format("http://{0}:{1}/connectRoomToZone?zoneUDN={2}&roomUDN={3}", mediaServer.IpAddress, RaumFeldStaticDefinitions.PORT_WEBSERVICE, zoneUdn, roomUdn);
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
                    {
                        request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                        request.Headers.Add("Accept-Language", "en");
                        request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            return true;
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                throw new Exception();

                await messagingService.ShowErrorDialogAsync(exception);
                return false;
            }
        }

        public async Task<bool> GetTuneInState()
        {
            if (mediaServer == null) { return false; }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //http://192.168.0.18:47365/238fc32a-6f11-4347-901e-3a81d4451989/TuneInSettings?action=open&documentTitle=TuneIn
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(string.Format("http://{0}:{1}/TuneInSettings?action=open&documentTitle=TuneIn", mediaServer.IpAddress, RaumFeldStaticDefinitions.PORT_WEBSERVICE))))
                    {
                        request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                        request.Headers.Add("Accept-Language", "en");
                        request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");
                        request.Method = HttpMethod.Get;

                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                            {
                                string htmlResponse = await response.Content.ReadAsStringAsync();
                                HtmlDocument resultat = new HtmlDocument();
                                resultat.LoadHtml(htmlResponse.Replace("<script language='JavaScript' src='/staticFiles/raumfeld.js' />", ""));

                                List<HtmlNode> nodes = resultat.DocumentNode.Descendants().Where(x => (x.Name == "div" && x.Attributes["onclick"] != null)).ToList();

                                foreach (var node in nodes)
                                {
                                    foreach (var attribute in node.Attributes)
                                    {
                                        if (attribute.Name == "onclick")
                                        {
                                            return attribute.Value == "loadURL(&apos;ToggleServiceActivation?activate=false&amp;service=TuneIn&apos;);" ? true : false;
                                        }
                                    }
                                }

                                return false;
                            }
                            else { return false; }
                        }
                    }
                }
            }

            catch (Exception)
            {
                throw new Exception();

                return false;
            }
        }
        public async Task<bool> SetTuneInState(bool state)
        {
            if (mediaServer == null) { return false; }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = string.Format("http://{0}:{1}/ToggleServiceActivation?activate={2}&service=TuneIn", mediaServer.IpAddress, RaumFeldStaticDefinitions.PORT_WEBSERVICE, (state ? "true" : "false"));
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
                    {
                        request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                        request.Headers.Add("Accept-Language", "en");
                        request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");

                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                            {
                                return state;
                            }
                            else { return false; }
                        }
                    }
                }
            }

            catch (Exception)
            {
                throw new Exception();

                return false;
            }
        }

        public async Task<bool> BrowseChildren(ObservableCollection<ElementBase> elements, string elementId, bool addItemsOnly)
        {
            var cancellationToken = await createNewToken();

            try
            {
                int start = 0;
                int limit = 10;
                bool continueLoop = true;
                elements.Clear();

                ServiceActionReturnMessage message;
                int index = 1;
                do
                {
                    message = await mediaServer.Browse(elementId, start, limit);

                    if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is List<ElementBase> partList)
                    {
                        if ((partList?.Count() ?? 0) > 0)
                        {
                            foreach (var item in partList)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                item.Index = index++;
                                if (addItemsOnly)
                                {
                                    if (!item.IsFolder)
                                    {
                                        elements.Add(item);
                                    }
                                }
                                else { elements.Add(item); }
                            }
                            start = start + limit;
                        }
                        else { continueLoop = false; }
                    }
                    else
                    {
                        elements.Clear();
                        continueLoop = false;
                    }
                } while (continueLoop);

                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception)
            {
                throw new Exception();

                return false;
            }
        }
        public async Task<ElementBase> BrowseMetaData(string elementId)
        {
            try
            {
                ServiceActionReturnMessage message;
                message = await mediaServer.BrowseMetaData(elementId);

                if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is ElementBase elementMetaData)
                {
                    return elementMetaData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw new Exception();

                return null;
            }
        }

        public async Task<bool> Search(ObservableCollection<ElementBase> elements, string elementId, string searchCriteria)
        {
            var cancellationToken = await createNewToken();

            try
            {
                int start = 0;
                int limit = 10;
                bool continueLoop = true;
                elements.Clear();

                ServiceActionReturnMessage message;
                do
                {
                    message = await mediaServer.Search(elementId, searchCriteria, start, limit);

                    if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is List<ElementBase> partList)
                    {
                        if ((partList?.Count() ?? 0) > 0)
                        {
                            foreach (var item in partList)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                elements.Add(item);
                            }
                            start = start + limit;
                        }
                        else { continueLoop = false; }
                    }
                    else
                    {
                        elements.Clear();
                        continueLoop = false;
                    }
                } while (continueLoop);

                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception)
            {
                throw new Exception();

                return false;
            }
        }

        #endregion
    }
}
