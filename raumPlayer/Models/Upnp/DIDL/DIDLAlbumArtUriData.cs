using System.Xml.Serialization;

namespace Upnp
{
    [XmlRoot("albumArtURI", Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
    public class DIDLAlbumArtUriData
    {
        [XmlAttribute("profileID", Namespace = "urn:schemas-dlna-org:metadata-1-0/")]
        public string ProfileId { get; set; }

        [XmlText]
        public string AlbumArtUri { get; set; }
    }
}
