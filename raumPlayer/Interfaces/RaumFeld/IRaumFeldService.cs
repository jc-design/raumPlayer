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

        Task<bool> BrowseChildren(ObservableCollection<IElement> elements, string id, bool addItemsOnly);
        Task<bool> BrowseMetaData(IElement element, string id);
        Task<bool> Search(ObservableCollection<IElement> elements, string id, string searchCriteria);
    }
}
