using raumPlayer.Interfaces;
using System.Collections.Generic;
using System.Xml.Serialization;
using Upnp;

namespace raumPlayer.Models
{
    public class RaumFeldEvent
    {
        public IMediaDevice MediaDevice { get; set; }
        public string EventSID { get; set; }
        public ServiceTypes SericeType { get; set; }
        public Dictionary<string,string> ChangedValues { get; set; }

        public RaumFeldEvent()
        {
            ChangedValues = new Dictionary<string, string>();
        }
    }
}
