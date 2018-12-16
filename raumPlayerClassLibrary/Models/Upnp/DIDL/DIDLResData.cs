using System.Xml.Serialization;

namespace Upnp
{
    [XmlRoot("res", Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/")]
    public class DIDLResData
    {
        [XmlText]
        public string Value { get; set; }

        [XmlAttribute("protocolInfo")]
        public string ProtocolInfo { get; set; }

        [XmlAttribute("resolution")]
        public string Resolution { get; set; }

        [XmlAttribute("size")]
        public string Size { get; set; }

        [XmlAttribute("duration")]
        public string Duration { get; set; }
    }
}
