using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class Device
    {
        public Device() { }

        [XmlElement("deviceType")]
        public string DeviceTypeText { get; set; }

        [XmlElement("UDN")]
        public string UDN { get; set; }

        [XmlElement("friendlyName")]
        public string FriendlyName { get; set; }

        [XmlElement("manufacturer")]
        public string Manufacturer { get; set; }

        [XmlElement("manufacturerURL")]
        public string ManufacturerURL { get; set; }

        [XmlElement("modelName")]
        public string ModelName { get; set; }

        [XmlElement("modelURL")]
        public string ModelURL { get; set; }

        [XmlElement("modelDescription")]
        public string ModelDescription { get; set; }

        [XmlElement("modelNumber")]
        public string ModelNumber { get; set; }

        [XmlElement("serialNumber")]
        public string SerialNumber { get; set; }

        [XmlElement("UPC")]
        public string UPC { get; set; }

        [XmlElement("presentationURL")]
        public string PresentationURL { get; set; }

        [XmlArray("iconList")]
        [XmlArrayItem("icon")]
        public Icon[] IconList { get; set; }

        [XmlArray("serviceList")]
        [XmlArrayItem("service")]
        public Service[] Services { get; set; }

        [XmlIgnore]
        public List<XmlElement> Any { get; set; }
    }
}
