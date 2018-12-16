using System.Xml.Serialization;
using Upnp;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    [XmlRoot("scpd", Namespace = "urn:schemas-upnp-org:service-1-0")]
    public class Services
    {
        public Services() { }

        [XmlElement("specVersion")]
        public SpecVersionType SpecVersion { get; set; }

        [XmlArray("actionList")]
        [XmlArrayItem("action")]
        public ServiceAction[] ActionList { get; set; }

        [XmlArray("serviceStateTable")]
        [XmlArrayItem("stateVariable")]
        public StateVariable[] ServiceStateTable { get; set; }

        public void SetParent()
        {
            foreach (var action in ActionList)
            {
                action.Parent = this;
            }

            foreach (var stateVariable in ServiceStateTable)
            {
                stateVariable.Parent = this;
            }
        }

    }
}
