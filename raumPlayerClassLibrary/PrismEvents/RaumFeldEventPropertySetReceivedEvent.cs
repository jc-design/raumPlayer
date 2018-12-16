using Prism.Events;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.PrismEvents
{
    public class RaumFeldEventPropertySetReceivedEvent : PubSubEvent<RaumFeldEventPropertySet>
    {
    }
}
