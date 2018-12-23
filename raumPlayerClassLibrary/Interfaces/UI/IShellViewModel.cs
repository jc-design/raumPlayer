using raumPlayer.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace raumPlayer.Interfaces
{
    public interface IShellViewModel
    {
        NavigationViewItem Selected { get; set; }
        string Header { get; set; }

        bool IsBusy { get; set; }
        Visibility BusyVisibility { get; set; }

        Task InitializeAsync(Frame frame, NavigationView navigationView);

        ZoneViewModel ActiveZoneViewModel { get; set; }
        ObservableCollection<ZoneViewModel> ZoneViewModels { get;}

        Task SetActiveZoneViewModel(string zoneUdn);

        //ICommand GetAlbumArtCommand { get; }
        //SolidColorBrush BackgroundColor { get; set; }
        //BitmapImage AlbumArtImage { get; set; }
    }
}
