using raumPlayer.Interfaces;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace raumPlayer.Models
{
    [XmlRoot("propertyset", Namespace = "urn:schemas-upnp-org:event-1-0")]
    public class RaumFeldEventPropertySet
    {
        [XmlElement("property")]
        public List<RaumFeldEventProperty> Properties { get; set; }

        [XmlIgnore]
        public string EventSID { get; set; }

        public RaumFeldEventPropertySet()
        {
            Properties = new List<RaumFeldEventProperty>();
        } 
    }
}
