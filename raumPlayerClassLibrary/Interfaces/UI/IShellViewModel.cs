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

        bool ZoneListAvailable { get; set; }
        bool IsBackEnabled { get; set; }
        bool IsTuneInAvailable { get; set; }
        bool IsBusy { get; set; }
        Visibility BusyVisibility { get; set; }

        void Initialize(Frame frame, NavigationView navigationView);

        ZoneViewModel ActiveZoneViewModel { get; set; }
        ObservableCollection<ZoneViewModel> ZoneViewModels { get;}

        ICommand ItemInvokedCommand { get; }
    }
}
