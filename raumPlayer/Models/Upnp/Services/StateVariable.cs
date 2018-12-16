using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class StateVariable
    {
        public StateVariable() { }

        [XmlIgnore]
        public Services Parent { get; set; } = null;

        [XmlAttribute("sendEvents")]
        public string SendEvents { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("dataType")]
        public string DataType { get; set; }

        [XmlArray("allowedValueList")]
        [XmlArrayItem("allowedValue")]
        public string[] AllowedValueList { get; set; }

        [XmlElement("aloowedvaluerange")]
        public AllowedValueRange AlloweValueRange { get; set; }
    }
}
