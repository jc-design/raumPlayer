using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    [XmlRoot("zoneConfig")]
    public class RaumFeldZoneConfig
    {
        public RaumFeldZoneConfig() { }

        [XmlAttribute("spotifyMode")]
        public string SpotifyMode { get; set; }

        [XmlAttribute("numRooms")]
        public string NumRooms { get; set; }

        [XmlArray("zones")]
        [XmlArrayItem("zone")]
        public ObservableCollection<RaumFeldZone> ZoneList { get; set; }

        [XmlArray("unassignedRooms")]
        [XmlArrayItem("room")]
        public ObservableCollection<RaumFeldRoom> UnAssignedRooms { get; set; }
    }
}
