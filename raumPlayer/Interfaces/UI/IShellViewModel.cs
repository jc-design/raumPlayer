using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        bool IsPaneOpen { get; set; }
        SplitViewDisplayMode DisplayMode { get; set; }

        object Selected { get; set; }
        string Header { get; set; }

        bool IsBusy { get; set; }
        Visibility BusyVisibility { get; set; }

        ObservableCollection<IShellNavigationItem> PrimaryItems { get; set; }
        ObservableCollection<IShellNavigationItem> SecondaryItems { get; set; }
        ObservableCollection<IShellNavigationItem> OtherItems { get; set; }

        ICommand ItemSelectedCommand { get; }
        ICommand SecondaryItemSelectedCommand { get; }
        ICommand OpenPaneCommand { get; }
        ICommand StateChangedCommand { get; }

        void Initialize(Frame frame);

        ZoneViewModel ActiveZoneViewModel { get; set; }
        ObservableCollection<ZoneViewModel> ZoneViewModels { get;}

        Task SetActiveZoneViewModel(string zoneUdn);
        void SetBackbuttonVisibility(bool state);
        void RefreshBinding(string binding);

        ICommand GetAlbumArtCommand { get; }
        SolidColorBrush BackgroundColor { get; set; }
        BitmapImage AlbumArtImage { get; set; }
    }
}
