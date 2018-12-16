using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    [XmlRoot("root", Namespace = "urn:schemas-upnp-org:device-1-0")]
    public class DeviceDescription
    {       
        public DeviceDescription() { }

        [XmlElement("specVersion")]
        public SpecVersionType SpecVersion { get; set; }

        [XmlElement("device")]
        public Device Device { get; set; }
    }
}
