using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Upnp;

namespace raumPlayer.Interfaces
{
    public interface IRaumFeldService
    {
        Task<bool> InitializeAsync();
        Task<bool> GetTuneInState();
        Task<bool> SetTuneInState(bool state);
        Task<bool> ConnectRoomToZone(string roomUdn, string zoneUdn);

        Task<bool> BrowseChildren(ObservableCollection<ElementBase> elements, string id, bool addItemsOnly);
        Task<ElementBase> BrowseMetaData(string id);
        Task<bool> Search(ObservableCollection<ElementBase> elements, string id, string searchCriteria);
    }
}
