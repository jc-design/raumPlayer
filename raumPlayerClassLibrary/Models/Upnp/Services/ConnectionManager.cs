using System.Xml.Serialization;

namespace Upnp
{
    [XmlRoot("scpd", Namespace = "urn:schemas-upnp-org:service-1-0")]
    public class ConnectionManager : Services { }
}
