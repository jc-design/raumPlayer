using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class Argument
    {
        public Argument() { }

        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("direction")]
        public string Direction { get; set; }
        [XmlElement("relatedStateVariable")]
        public string RelatedStateVariable { get; set; }

        [XmlIgnore]
        public string Value { get; set; }
    }


}
