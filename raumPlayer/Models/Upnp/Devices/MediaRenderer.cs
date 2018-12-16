using Prism.Events;
using raumPlayer;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.System.Threading;
using Windows.Web.Http;

namespace Upnp
{
    public class MediaRenderer : IMediaDevice
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

        public MediaRenderer(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, INetWorkSubscriber netWorkSubscriberInstance, DeviceDescription deviceDescription, string ipAddress, int port, MediaDeviceType deviceType)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            netWorkSubscriber = netWorkSubscriberInstance;

            this.deviceDescription = deviceDescription;
            this.ipAddress = ipAddress;
            this.port = port;

            DeviceType = deviceType;

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
            if (AVTransport != null)
            {
                foreach (ServiceAction act in AVTransport.ActionList)
                {
                    ServiceActions[act.Name.ToUpper()] = act;
                }
            }

            if (RenderingControl != null)
            {
                foreach (ServiceAction act in RenderingControl.ActionList)
                {
                    ServiceActions[act.Name.ToUpper()] = act;
                }
            }
        }

        #region AVTransport Methods

        public async Task<DIDLLite> BendAVTransportUri(string id)
        {
            try
            {
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set Room on standby
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> EnterManualStandby(string roomUUID)
        {
            try
            {
                if (ServiceActions.TryGetValue("ENTERMANUALSTANDBY", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);
                    
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);

                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetCurrentTransportActions") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get the possible transport actions
        /// </summary>
        /// <returns>String[] with all possible actions</returns>
        public async Task<ServiceActionReturnMessage> GetCurrentTransportActions()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETCURRENTTRANSPORTACTIONS", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        string result = action.GetArgumentValue("Actions");

                        string[] stringSeparators = new string[] { "," };
                        // Split delimited by another string and return all non-empty elements.
                        message.ReturnValue = result.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetCurrentTransportActions") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        ///// <summary>
        ///// GetDeviceCapabilities
        ///// </summary>
        ///// <returns></returns>
        //public async Task<DIDLLite> GetDeviceCapabilities()
        //{
        //    try
        //    {
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// Get media info of current track
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> GetMediaInfo()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETMEDIAINFO", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        Dictionary<string, string> mediaInfo = new Dictionary<string, string>
                        {
                            ["NrTracks"] = action.GetArgumentValue("NrTracks"),
                            ["CurrentURIMetaData"] = action.GetArgumentValue("CurrentURIMetaData")
                        };
                        message.ReturnValue = mediaInfo;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetMediaInfo") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get info of current track
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> GetPositionInfo()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETPOSITIONINFO", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        Dictionary<string, string> positionInfo = new Dictionary<string, string>
                        {
                            ["Track"] = action.GetArgumentValue("Track"),
                            ["TrackDuration"] = action.GetArgumentValue("TrackDuration"),
                            ["TrackMetaData"] = action.GetArgumentValue("TrackMetaData"),
                            ["TrackURI"] = action.GetArgumentValue("TrackUri")
                        };
                        message.ReturnValue = positionInfo;
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetPositionInfo") };
                }

            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        public async Task<DIDLLite> GetStreamProperties(string id)
        {
            try
            {
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get current playmode
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> GetTransportSettings()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETTRANSPORTSETTINGS", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        message.ReturnValue = action.GetArgumentValue("Playmode");
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetTransportSettings") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Access denied?!
        /// </summary>
        /// <returns></returns>
        //public async Task<DIDLLite> LikeCurrent()
        //{
        //    try
        //    {
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// Activate Room
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> LeaveStandby(string roomUUID)
        {
            try
            {
                if (ServiceActions.TryGetValue("LEAVESTANDBY", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);

                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetCurrentTransportActions") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Play next song
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Next()
        {
            try
            {
                if (ServiceActions.TryGetValue("NEXT", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Next") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }
        /// <summary>
        /// Pause playing
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Pause()
        {
            try
            {
                if (ServiceActions.TryGetValue("PAUSE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Pause") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Start playing
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Play()
        {
            try
            {
                if (ServiceActions.TryGetValue("PLAY", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Play") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Play previous song
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Previous()
        {
            try
            {
                if (ServiceActions.TryGetValue("PREVIOUS", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Previous") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Seek Tracknumber of current container
        /// </summary>
        /// <param name="unit">Possible values "ABS_TIME", "REL_RIME", "TRACK_NR"; only "TRACK_NR" is working for musiccontainer </param>
        /// <param name="target">New Tracknumber</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Seek(string unit, string target)
        {
            try
            {
                if (ServiceActions.TryGetValue("SEEK", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Unit", unit);
                    action.SetArgumentValue("Target", target);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Seek") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUDN"></param>
        /// <param name="id"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetAVTransportUri(bool isContainer, string serverUDN, string containerid, string firstitemid, int index, string metaData)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETAVTRANSPORTURI", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("CurrentURI", HtmlExtension.BuildAvTransportUri(isContainer, serverUDN, containerID: containerid, firstItemID: firstitemid, firstItemIndex: index));
                    action.SetArgumentValue("CurrentURIMetaData", WebUtility.HtmlEncode(metaData));
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetAVTransportUri") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Set playmode
        /// </summary>
        /// <param name="playmode">"NORMAL", "SHUFFLE", "REPEAT_ONE", "REPEAT_ALL", "RANDOM", "DIRECT_1", "INTRO"</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetPlayMode(string playmode)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETPLAYMODE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("NewPlayMode", playmode);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetPlayMode") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DIDLLite> SetRessourceForCurrentStream(string id)
        {
            try
            {
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> Stop()
        {
            try
            {
                if (ServiceActions.TryGetValue("STOP", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.AVTRANSPORT, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.AVTRANSPORT).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: Stop") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        //public async Task<DIDLLite> UnlikeCurrent(string id)
        //{
        //    try
        //    {
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        #endregion

        #region RenderControl Methods

        /// <summary>
        /// Change main volume
        /// </summary>
        /// <param name="amount">From -127 to 127, Step 1</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> ChangeVolume(string amount)
        {
            try
            {
                if (ServiceActions.TryGetValue("CHANGEVOLUME", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Amount", amount);

                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: ChangeVolume") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get main mute status
        /// </summary>
        /// <returns>current muteStatus</returns>
        public async Task<ServiceActionReturnMessage> GetMute()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETMUTE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Channel", "Master");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        message.ReturnValue = action.GetArgumentValue("CurrentMute") == "1";
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetMute") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get room mute status
        /// </summary>
        /// <param name="roomUUID">Room UUID; string starts with "uuid:..."</param>
        /// <returns>current muteStatus</returns>
        public async Task<ServiceActionReturnMessage> GetRoomMute(string roomUUID)
        {
            try
            {
                if (ServiceActions.TryGetValue("GETROOMMUTE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        message.ReturnValue = action.GetArgumentValue("CurrentMute") == "1";
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetRoomMute") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get room volume
        /// </summary>
        /// <param name="roomUUID">Room UUID; string starts with "uuid:..."</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> GetRoomVolume(string roomUUID)
        {
            try
            {
                if (ServiceActions.TryGetValue("GETROOMVOLUME", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        message.ReturnValue = double.Parse(action.GetArgumentValue("CurrentVolume"));
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetRoomVolume") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Get main volume
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> GetVolume()
        {
            try
            {
                if (ServiceActions.TryGetValue("GETVOLUME", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Channel", "Master");
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    if (message.ActionStatus == ActionStatus.Okay)
                    {
                        message.ReturnValue = double.Parse(action.GetArgumentValue("CurrentVolume"));
                        return message;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: GetVolume") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Set main mute status
        /// </summary>
        /// <param name="muteStatus">new muteStatus</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetMute(string muteStatus)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETMUTE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Channel", "Master");
                    action.SetArgumentValue("DesiredMute", muteStatus);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetMute") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Set room mute status
        /// </summary>
        /// <param name="roomUUID">Room UUID; string starts with "uuid:..."</param>
        /// <param name="muteStatus">new muteStatus</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetRoomMute(string roomUUID, string muteStatus)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETROOMMUTE", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);
                    action.SetArgumentValue("DesiredMute", muteStatus);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetRoomMute") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Set room volume
        /// </summary>
        /// <param name="roomUUID">Room UUID; string starts with "uuid:..."</param>
        /// <param name="volume">values from 0 to 100</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetRoomVolume(string roomUUID, string volume)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETROOMVOLUME", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Room", roomUUID);
                    action.SetArgumentValue("DesiredVolume", volume);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetRoomVolume") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        /// <summary>
        /// Set main volume
        /// </summary>
        /// <param name="roomUUID">Room UUID; string starts with "uuid:..."</param>
        /// <param name="volume">values from 0 to 100</param>
        /// <returns></returns>
        public async Task<ServiceActionReturnMessage> SetVolume(string volume)
        {
            try
            {
                if (ServiceActions.TryGetValue("SETVOLUME", out ServiceAction action))
                {
                    action.ClearArgumentsValue();
                    action.SetArgumentValue("InstanceId", "0");
                    action.SetArgumentValue("Channel", "Master");
                    action.SetArgumentValue("DesiredVolume", volume);
                    ServiceActionReturnMessage message = await action.InvokeAsync(ServiceTypesString.RENDERINGCONTROL, serviceControls.Select(c => c).Where(c => c.ServiceType == ServiceTypes.RENDERINGCONTROL).FirstOrDefault().URI);
                    return message;
                }
                else
                {
                    return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = string.Format("Action not available: SetVolume") };
                }
            }
            catch (Exception exception)
            {
                return new ServiceActionReturnMessage() { ActionStatus = ActionStatus.Error, ActionMessage = exception.Message };
            }
        }

        #endregion

    }
}
