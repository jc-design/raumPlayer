using System.Collections.Generic;
using System.Xml.Serialization;

namespace Upnp
{
    [XmlRoot("DIDL-Lite", Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/")]
    public class DIDLLite
    {
        [XmlElement("container")]
        public List<DIDLContainer> Containers { get; set; }
        [XmlElement("item")]
        public List<DIDLItem> Items { get; set; }
    }
}
