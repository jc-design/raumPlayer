using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    public class RaumFeldRenderer
    {
        public RaumFeldRenderer() { }

        [XmlAttribute("udn")]
        public string Udn { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
