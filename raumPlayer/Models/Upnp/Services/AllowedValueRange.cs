using System.Xml.Serialization;

namespace Upnp
{
    public class AllowedValueRange
    {
        public AllowedValueRange() { }

        [XmlElement("minimum")]
        public string Minimum { get; set; }
        [XmlElement("maximum")]
        public string Maximum { get; set; }
        [XmlElement("step")]
        public string Step { get; set; }

    }
}
