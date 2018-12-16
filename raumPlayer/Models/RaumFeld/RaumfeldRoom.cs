using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    public class RaumFeldRoom
    {
        public RaumFeldRoom() { }

        [XmlAttribute("udn")]
        public string Udn { get; set; }

        [XmlAttribute("powerState")]
        public string PowerState { get; set; }

        [XmlAttribute("color")]
        public string Color { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("renderer")]
        public RaumFeldRenderer Renderer { get; set; }
    }
}
