using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    public class RaumfeldDevice
    {
        public RaumfeldDevice() { }

        [XmlAttribute("udn")]
        public string Udn { get; set; }

        [XmlAttribute("location")]
        public string Location { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Name { get; set; }
    }
}
