using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upnp;

namespace raumPlayer.Models
{
    public class NetWorkSubscriberPayload
    {
        public IMediaDevice MediaDevice { get; set; }
        public string URI { get; set; }
        public ServiceTypes ServiceType { get; set; }
    }
}
