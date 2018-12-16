using raumPlayer.Services;
using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Upnp;

namespace raumPlayer.Interfaces
{
    public interface IRoomViewModel
    {
        string Udn { get; }
        string Name { get; }

        string PowerState { get; set; }
        double RoomVolume { get; set; }
        bool? RoomMute { get; set; }
        bool IsActive { get; set; }

        ICommand GetRoomVolumeCommand { get; }
        ICommand GetRoomMuteCommand { get; }
        ICommand SetRoomVolumeCommand { get; }
        ICommand SetRoomMuteCommand { get; }
        ICommand SetRoomStateCommand { get; }
    }
}
