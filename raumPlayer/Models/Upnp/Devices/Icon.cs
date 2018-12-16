using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Upnp
{
    public class Icon
    {
        public Icon() { }

        [XmlElement("mimetype")]
        public string MimeType { get; set; }

        [XmlElement("height")]
        public int Height { get; set; }

        [XmlElement("width")]
        public int Width { get; set; }

        [XmlElement("depth")]
        public int Depth { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }
    }
}
