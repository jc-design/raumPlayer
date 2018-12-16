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
    public class SystemUpdateIDChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class MuteChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class VolumeChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class RoomMutesChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class RoomVolumesChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class AVTransportURIChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class AVTransportURIMetaDataChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class CurrentTrackChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class CurrentTrackURIChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class CurrentPlayModeChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class CurrentTrackMetaDataChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class CurrentTransportActionsChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class NumberOfTracksChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class PowerStateChangedEvent : PubSubEvent<RaumFeldEvent> { }
    public class RoomStatesChangedEvent : PubSubEvent<RaumFeldEvent> { }
}
