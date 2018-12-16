using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class SpecVersionType
    {
        public SpecVersionType() { }

        [XmlElement("major")]
        public int Major { get; set; }

        [XmlElement("minor")]
        public int Minor { get; set; }
    }
}
