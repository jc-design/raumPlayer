using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace raumPlayer.Models
{
    [XmlRoot("devices")]
    public class RaumfeldDevices
    {
        public RaumfeldDevices() { }

        [XmlElement("device")]
        public RaumfeldDevice[] Devices { get; set; }
    }
}
