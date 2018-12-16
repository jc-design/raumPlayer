using raumPlayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Upnp;
using Windows.System.Threading;

namespace raumPlayer.Interfaces
{
    public interface INetWorkSubscriber
    {
        /// <summary>
        /// Key:   NetWorkSubscriberPayload
        /// Value: EventSID
        /// </summary>
        Dictionary<NetWorkSubscriberPayload, string> SubscriberDictionary { get; set; }

        ThreadPoolTimer Timer { get; set; }
        Task Subscribe(List<NetWorkSubscriberPayload> serviceEvents);
        Task UnSubscribe();
    }
}
