using Prism.Events;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.PrismEvents
{
    public class RaumFerldZonesLoadedEvent : PubSubEvent<IList<ZoneViewModel>> { }
}
