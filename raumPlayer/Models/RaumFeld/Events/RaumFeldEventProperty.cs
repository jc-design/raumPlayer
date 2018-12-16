using raumPlayer.Interfaces;
using System.Xml.Serialization;

namespace raumPlayer.Models
{
    [XmlRoot(Namespace = "")]
    public class RaumFeldEventProperty
    {
        [XmlElement("LastChange")]
        public string LastChange { get; set; }

        [XmlElement("SinkProtocolInfo")]
        public string SinkProtocolInfo { get; set; }

        [XmlElement("IndexerStatus")]
        public string IndexerStatus { get; set; }

        [XmlElement("SystemUpdateID")]
        public string SystemUpdateID { get; set; }
    }
}
