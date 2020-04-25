using Prism.Events;
using raumPlayer.Interfaces;
using raumPlayer.PrismEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upnp;
using Windows.Data.Xml.Dom;
using Windows.System.Threading;
using Windows.Web.Http;

namespace raumPlayer.Models
{
    public class NetWorkSubscriber : INetWorkSubscriber
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INetWorkSocketListener netWorkSocketListener;

        public Dictionary<NetWorkSubscriberPayload, string> SubscriberDictionary { get; set; }
        public ThreadPoolTimer Timer { get; set; }

        public NetWorkSubscriber(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, INetWorkSocketListener netWorkSocketListenerInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            netWorkSocketListener = netWorkSocketListenerInstance;

            SubscriberDictionary = new Dictionary<NetWorkSubscriberPayload, string>();

            eventAggregator.GetEvent<RaumFeldEventPropertySetReceivedEvent>().Subscribe(onRaumFeldEventPropertySetReceived);
        }

        public async Task Subscribe(List<NetWorkSubscriberPayload> serviceEvents)
        {
            if (string.IsNullOrEmpty(netWorkSocketListener.Hostname)) { return; }

            foreach (var payload in serviceEvents)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("SUBSCRIBE"), new Uri(payload.URI));
                    request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                    request.Headers.Add("Accept-Language", "en");
                    request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");
                    request.Headers.Add("CALLBACK", "<" + netWorkSocketListener.Hostname + ">");
                    request.Headers.Add("NT", "upnp:event");
                    request.Headers.Add("TIMEOUT", "Second-300");

                    HttpResponseMessage response = await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                    {
                        SubscriberDictionary[payload] = response.Headers["SID"];
                        if (Timer == null) { Timer = ThreadPoolTimer.CreatePeriodicTimer(async (t) => { await updateSubscribe(); }, TimeSpan.FromSeconds(240)); }
                    }
                    else
                    {
                        await messagingService.ShowErrorDialogAsync(string.Format("{0}: {1}", response.StatusCode, await response.Content.ReadAsStringAsync()));
                    }
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
        }
        private async Task subscribe(NetWorkSubscriberPayload payload)
        {
            if (string.IsNullOrEmpty(netWorkSocketListener.Hostname)) { return; }

            try
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("SUBSCRIBE"), new Uri(payload.URI));
                request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                request.Headers.Add("Accept-Language", "en");
                request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");
                request.Headers.Add("CALLBACK", "<" + netWorkSocketListener.Hostname + ">");
                request.Headers.Add("NT", "upnp:event");
                request.Headers.Add("TIMEOUT", "Second-300");

                HttpResponseMessage response = await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                {
                    SubscriberDictionary[payload] = response.Headers["SID"];
                    if (Timer == null) { Timer = ThreadPoolTimer.CreatePeriodicTimer(async (t) => { await updateSubscribe(); }, TimeSpan.FromSeconds(240)); }
                }
                else
                {
                    await messagingService.ShowErrorDialogAsync(string.Format("{0}: {1}", response.StatusCode, await response.Content.ReadAsStringAsync()));
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
        private async Task updateSubscribe()
        {
            foreach (var keyvaluePair in SubscriberDictionary)
            {
                try
                {
                    if (SubscriberDictionary.TryGetValue(keyvaluePair.Key, out string sid))
                    {
                        HttpClient httpClient = new HttpClient();
                        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("SUBSCRIBE"), new Uri(keyvaluePair.Key.URI));
                        request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                        request.Headers.Add("Accept-Language", "en");
                        request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");
                        request.Headers.Add("SID", sid);
                        request.Headers.Add("TIMEOUT", "Second-300");

                        HttpResponseMessage response = await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);

                        if (response.StatusCode != Windows.Web.Http.HttpStatusCode.Ok)
                        {
                            //SubscriberDictionary.Remove(keyvaluePair.Key);
                            await subscribe(keyvaluePair.Key);
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
        }
        public async Task UnSubscribe()
        {
            if (Timer != null) { Timer.Cancel(); }

            foreach (var keyvaluePair in SubscriberDictionary)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("UNSUBSCRIBE"), new Uri(keyvaluePair.Key.URI));
                    request.Headers.Add("User-Agent", "RaumfeldControl/0.0 RaumfeldProtocol/1");    /* RaumfeldControl/3.6 RaumfeldProtocol/399 Build => https://github.com/masmu/pulseaudio-dlna/issues/227 */
                    request.Headers.Add("Accept-Language", "en");
                    request.Headers.Add("ContentType", "text/xml; charset=\"utf - 8\"");
                    request.Headers.Add("SID", keyvaluePair.Value);

                    HttpResponseMessage response = await httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    //SubscriberDictionary.Remove(keyvaluePair.Key);
                    if (Timer != null)
                    {
                        Timer.Cancel();
                        Timer = null;
                    }
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }

            SubscriberDictionary.Clear();
        }

        private void onRaumFeldEventPropertySetReceived(RaumFeldEventPropertySet args)
        {
            NetWorkSubscriberPayload payload = SubscriberDictionary.Select(d => d).Where(d => d.Value == args.EventSID).FirstOrDefault().Key;
            if (payload == null) { return; }

            foreach (var property in args.Properties)
            {
                if (property.SystemUpdateID != null)
                {
                    RaumFeldEvent raumFeldEvent = new RaumFeldEvent()
                    {
                        MediaDevice = payload.MediaDevice,
                        EventSID = args.EventSID,
                    };
                    raumFeldEvent.ChangedValues.Add("SystemUpdateID", property.SystemUpdateID);
                    eventAggregator.GetEvent<SystemUpdateIDChangedEvent>().Publish(raumFeldEvent);
                }
                if (property.LastChange != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(property.LastChange);
                    XmlNodeList xmlNodes = xmlDocument.SelectNodes("//*");

                    foreach (var item in xmlNodes)
                    {
                        RaumFeldEvent raumFeldEvent = new RaumFeldEvent()
                        {
                            MediaDevice = payload.MediaDevice,
                            EventSID = args.EventSID,
                        };

                        switch ((string)item.NamespaceUri)
                        {
                            case "urn:schemas-upnp-org:metadata-1-0/RCS/":
                                raumFeldEvent.SericeType = ServiceTypes.RENDERINGCONTROL;
                                break;
                            case "urn:schemas-upnp-org:metadata-1-0/AVT/":
                                raumFeldEvent.SericeType = ServiceTypes.AVTRANSPORT;
                                break;
                            default:
                                break;
                        }

                        var values = new Dictionary<string, string>();
                        foreach (var a in item.Attributes)
                        {
                            raumFeldEvent.ChangedValues.Add(a.NodeName, (string)a.NodeValue);
                        }

                        switch (item.NodeName)
                        {
                            // RenderingControlEvents
                            case "Mute":
                                eventAggregator.GetEvent<MuteChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "Volume":
                                eventAggregator.GetEvent<VolumeChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "RoomMutes":
                                eventAggregator.GetEvent<RoomMutesChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "RoomVolumes":
                                eventAggregator.GetEvent<RoomVolumesChangedEvent>().Publish(raumFeldEvent);
                                break;

                            // AVTransportEvents
                            case "AVTransportURI":
                                eventAggregator.GetEvent<AVTransportURIChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "AVTransportURIMetaData":
                                eventAggregator.GetEvent<AVTransportURIMetaDataChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "CurrentTrack":
                                eventAggregator.GetEvent<CurrentTrackChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "CurrentTrackURI":
                                eventAggregator.GetEvent<CurrentTrackURIChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "CurrentPlayMode":
                                eventAggregator.GetEvent<CurrentPlayModeChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "CurrentTrackMetaData":
                                eventAggregator.GetEvent<CurrentTrackMetaDataChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "CurrentTransportActions":
                                eventAggregator.GetEvent<CurrentTransportActionsChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "NumberOfTracks":
                                eventAggregator.GetEvent<NumberOfTracksChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "PowerState":
                                eventAggregator.GetEvent<PowerStateChangedEvent>().Publish(raumFeldEvent);
                                break;
                            case "RoomStates":
                                eventAggregator.GetEvent<RoomStatesChangedEvent>().Publish(raumFeldEvent);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
