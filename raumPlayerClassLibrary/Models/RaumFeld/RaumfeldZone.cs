using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    public class RaumFeldZone
    {
        [XmlAttribute("udn")]
        public string Udn { get; set; }

        [XmlElement("room")]
        public ObservableCollection<RaumFeldRoom> Rooms { get; set; }
    }
}
