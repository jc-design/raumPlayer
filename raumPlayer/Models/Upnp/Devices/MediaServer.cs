using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Upnp;

namespace Upnp
{
    public class MediaServer : IMediaDevice
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INetWorkSubscriber netWorkSubscriber;

        private readonly DeviceDescription deviceDescription;
        private readonly string ipAddress;
        private readonly int port;

        private List<NetWorkSubscriberPayload> serviceSCPDs;
        private List<NetWorkSubscriberPayload> serviceControls;
        private List<NetWorkSubscriberPayload> serviceEvents;

        public AVTransport AVTransport { get; set; }
        public ConnectionManager ConnectionManager { get; set; }
        public ContentDirectory ContentDirectory { get; set; }
        public MediaReceiverRegistrar MediaReceiverRegistrar { get; set; }
        public RenderingControl RenderingControl { get; set; }

        public string Name { get { return deviceDescription.Device.FriendlyName; } }
        public string Udn { get { return deviceDescription.Device.UDN; } }
        public string IpAddress { get { return ipAddress; } }

        public MediaDeviceType DeviceType { get; set; }

        public Dictionary<string, ServiceAction> ServiceActions { get; set; }

        public MediaServer(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, INetWorkSubscriber netWorkSubscriberInstance, DeviceDescription deviceDescription, string ipAddress, int port)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            netWorkSubscriber = netWorkSubscriberInstance;

            this.deviceDescription = deviceDescription;
            this.ipAddress = ipAddress;
            this.port = port;

            DeviceType = MediaDeviceType.MediaServer;

            //Key: Action.Name, Value: ServiceAction
            ServiceActions = new Dictionary<string, ServiceAction>();

            //Key: ServiceType, Value: Uri-String
            serviceSCPDs = new List<NetWorkSubscriberPayload>();
            serviceControls = new List<NetWorkSubscriberPayload>();
            serviceEvents = new List<NetWorkSubscriberPayload>();
        }

        public async Task InitializeAsync()
        {
            await loadServicesAsync();
            assignActions();
            await netWorkSubscriber.Subscribe(serviceEvents);
        }

        private async Task loadServicesAsync()
        {
            ServiceTypes serviceType;

            foreach (Service service in deviceDescription.Device.Services)
            {
                switch (service.ServiceType)
                {
                    case ServiceTypesString.AVTRANSPORT:
                        AVTransport = await XmlExtension.DeserializeUriAsync<AVTransport>(new Uri(HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL)));
                        AVTransport.SetParent();
                        serviceType = ServiceTypes.AVTRANSPORT;
                        break;
                    case ServiceTypesString.CONNECTIONMANAGER:
                        ConnectionManager = await XmlExtension.DeserializeUriAsync<ConnectionManager>(new Uri(HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL)));
                        ConnectionManager.SetParent();
                        serviceType = ServiceTypes.CONNECTIONMANAGER;
                        break;
                    case ServiceTypesString.CONTENTDIRECTORY:
                        ContentDirectory = await XmlExtension.DeserializeUriAsync<ContentDirectory>(new Uri(HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL)));
                        ContentDirectory.SetParent();
                        serviceType = ServiceTypes.CONTENTDIRECTORY;
                        break;
                    case ServiceTypesString.MEDIARECEIVERREGISTRAR:
                        MediaReceiverRegistrar = await XmlExtension.DeserializeUriAsync<MediaReceiverRegistrar>(new Uri(HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL)));
                        MediaReceiverRegistrar.SetParent();
                        serviceType = ServiceTypes.MEDIARECEIVERREGISTRAR;
                        break;
                    case ServiceTypesString.RENDERINGCONTROL:
                        RenderingControl = await XmlExtension.DeserializeUriAsync<RenderingControl>(new Uri(HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL)));
                        RenderingControl.SetParent();
                        serviceType = ServiceTypes.RENDERINGCONTROL;
                        break;
                    default:
                        serviceType = ServiceTypes.NEUTRAL;
                        break;
                }

                if (serviceType != ServiceTypes.NEUTRAL)
                {
                    serviceSCPDs.Add(new NetWorkSubscriberPayload { MediaDevice = this, URI = HtmlExtension.CompleteHttpString(ipAddress, port, service.SCPDURL), ServiceType = serviceType, });
                    serviceControls.Add(new NetWorkSubscriberPayload { MediaDevice = this, URI = HtmlExtension.CompleteHttpString(ipAddress, port, service.ControlURL), ServiceType = serviceType, });
                    serviceEvents.Add(new NetWorkSubscriberPayload { MediaDevice = this, URI = HtmlExtension.CompleteHttpString(ipAddress, port, service.EventSubURL), ServiceType = serviceType, });
                }
            }
        }
        private void assignActions()
        {
            if (ContentDirectory != null)
            {
                foreach (ServiceAction act in ContentDirectory.ActionList)
                {
                    ServiceActions[act.Name.ToUpper()] = act;
                    //if (!ServiceActions.ContainsKey(act.Name.ToUpper())) { ServiceActions.Add(act.Name.ToUpper(), act); }
                }
            }

            if (ConnectionManager != null)
            {
                foreach (ServiceAction act in ConnectionManager.ActionList)
                {
                    ServiceActions[act.Name.ToUpper()] = act;
                    //if (!ServiceActions.ContainsKey(act.Name.ToUpper())) { ServiceActions.Add(act.Name.ToUpper(), act); }
                }
            }
        }

        /// <summary>
        /// Add container to queue
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> AddContainerToQueue(string containerId, int position)
        {
            try
            {
                if (ServiceActions.TryGetValue("ADDCONTAINERTOQUEUE", out ServiceAction action))
                {

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("QueueID", RaumFeldStaticDefinitions.PLAYLISTBASE + "/" + RaumFeldStaticDefinitions.PLAYLIST);
                    action.SetArgumentValue("ContainerID", containerId);
                    action.SetArgumentValue("SourceID", "");
                    action.SetArgumentValue("SearchCriteria", "");
                    action.SetArgumentValue("SortCriteria", "");
                    action.SetArgumentValue("StartIndex", "0");
                    action.SetArgumentValue("EndIndex", "2147483647");
                    action.SetArgumentValue("Position", position.ToString());

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Browse") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Add item to queue
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> AddItemToQueue(string containerId, int position)
        {
            try
            {
                if (ServiceActions.TryGetValue("ADDITEMTOQUEUE", out ServiceAction action))
                {

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("QueueID", RaumFeldStaticDefinitions.PLAYLIST);
                    action.SetArgumentValue("ContainerID", containerId);
                    action.SetArgumentValue("Position", position.ToString());

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Browse") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Browse
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="flag"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Browse(string containerId, int start, int limit = 10)
        {

            try
            {
                if (ServiceActions.TryGetValue("BROWSE", out ServiceAction action))
                {
                    List<IElement> elements = new List<IElement>();

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("ObjectID", containerId);
                    action.SetArgumentValue("BrowseFlag", "BrowseDirectChildren");
                    action.SetArgumentValue("Filter", "*");
                    action.SetArgumentValue("StartingIndex", start.ToString());
                    action.SetArgumentValue("RequestedCount", limit.ToString());
                    action.SetArgumentValue("SortCriteria", "");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        string result = action.GetArgumentValue("Result");
                        DIDLLite didlliteResult = result.Deserialize<DIDLLite>();

                        if (string.IsNullOrEmpty(result) || didlliteResult == null) { return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Parsing error DIDLLite-string") }; }

                        foreach (DIDLContainer container in didlliteResult.Containers)
                        {
                            switch (container.Title)
                            {
                                case "Zones":
                                case "Renderers":
                                case "Search":
                                    break;
                                default:
                                    IElement element = PrismUnityApplication.Current.Container.Resolve<ElementContainer>(new ResolverOverride[]
                                        {
                                           new ParameterOverride("didl", container)
                                        });
                                    elements.Add(element);

                                    break;
                            }

                        }
                        foreach (DIDLItem item in didlliteResult.Items)
                        {
                            IElement element = PrismUnityApplication.Current.Container.Resolve<ElementItem>(new ResolverOverride[]
                                        {
                                           new ParameterOverride("didl", item)
                                        });
                            elements.Add(element);
                        }

                        message.ReturnValue = elements;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Browse") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// BrowseMetaData
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> BrowseMetaData(string Id)
        {

            try
            {
                if (ServiceActions.TryGetValue("BROWSE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("ObjectID", Id);
                    action.SetArgumentValue("BrowseFlag", "BrowseMetadata");
                    action.SetArgumentValue("Filter", "*");
                    action.SetArgumentValue("StartingIndex", "0");
                    action.SetArgumentValue("RequestedCount", "0");
                    action.SetArgumentValue("SortCriteria", "");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        string result = action.GetArgumentValue("Result");

                        message.ReturnValue = result;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Browse") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Create new queue in playlists
        /// To be able to add songs and reorder items
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> CreateQueue()
        {
            try
            {
                if (ServiceActions.TryGetValue("CREATEQUEUE", out ServiceAction action))
                {

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("DesiredName", RaumFeldStaticDefinitions.PLAYLIST);
                    action.SetArgumentValue("ContainerID", RaumFeldStaticDefinitions.PLAYLISTBASE);

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        Dictionary<string, string> results = new Dictionary<string, string>
                        {
                            ["GivenName"] = action.GetArgumentValue("GivenName"),
                            ["QueueID"] = action.GetArgumentValue("QueueID")
                        };

                        message.ReturnValue = results;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: CreateQueue") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Create a reference in favorites
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> CreateReference(string objectId)
        {
            try
            {
                if (ServiceActions.TryGetValue("CREATEREFERENCE", out ServiceAction action))
                {

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("ContainerID", "0/Favorites/MyFavorites");
                    action.SetArgumentValue("ObjectID", objectId);

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: CreateReference") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Destroy Object
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> DestroyObject(string objectId)
        {
            try
            {
                if (ServiceActions.TryGetValue("DESTROYOBJECT", out ServiceAction action))
                {

                    action.ClearArgumentsValue();
                    action.SetArgumentValue("ObjectID", objectId);

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: DestroyObject") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Search elements
        /// Important: Search only works within the right containerID
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Search(string containerId, string searchCriteria, int start, int limit = 10)
        {

            try
            {
                if (ServiceActions.TryGetValue("SEARCH", out ServiceAction action))
                {
                    List<IElement> elements = new List<IElement>();

                    //bool found = false;
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("ContainerID", containerId);
                    action.SetArgumentValue("SearchCriteria", string.Format("dc:title contains \"{0}\"", searchCriteria));
                    action.SetArgumentValue("Filter", "*");
                    action.SetArgumentValue("StartingIndex", start.ToString());
                    action.SetArgumentValue("RequestedCount", limit.ToString());
                    action.SetArgumentValue("SortCriteria", "");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.CONTENTDIRECTORY, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.CONTENTDIRECTORY).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        string result = action.GetArgumentValue("Result");
                        DIDLLite didlliteResult = result.Deserialize<DIDLLite>();

                        if (string.IsNullOrEmpty(result) || didlliteResult == null) { return null; }

                        foreach (DIDLContainer container in didlliteResult.Containers)
                        {
                            switch (container.Title)
                            {
                                case "Zones":
                                case "Renderers":
                                case "Search":
                                    break;
                                default:
                                    IElement element = Prism.Unity.Windows.PrismUnityApplication.Current.Container.Resolve<ElementContainer>(new ResolverOverride[]
                                        {
                                           new ParameterOverride("didl", container)
                                        });
                                    elements.Add(element);

                                    break;
                            }

                        }
                        foreach (DIDLItem item in didlliteResult.Items)
                        {
                            IElement element = Prism.Unity.Windows.PrismUnityApplication.Current.Container.Resolve<ElementItem>(new ResolverOverride[]
                                        {
                                           new ParameterOverride("didl", item)
                                        });
                            elements.Add(element);
                        }

                        message.ReturnValue = elements;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Browse") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }
    }
}
