using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class Service
    {
        public Service() { }

        [XmlElement("serviceType")]
        public string ServiceType { get; set; }

        [XmlElement("serviceId")]
        public string ServiceId { get; set; }

        [XmlElement("SCPDURL")]
        public string SCPDURL { get; set; }

        [XmlElement("eventSubURL")]
        public string EventSubURL { get; set; }

        [XmlElement("controlURL")]
        public string ControlURL { get; set; }
    }
}
