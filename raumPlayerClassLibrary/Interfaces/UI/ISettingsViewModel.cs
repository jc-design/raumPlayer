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
    public interface ISettingsViewModel
    {
        ElementTheme ElementTheme { get; set; }

        string VersionDescription { get; set; }

        bool IsTuneInAvailable { get; set; }
        bool IsCheckedMyMusic { get; set; }
        bool IsCheckedFavorites { get; set; }

        ICommand SwitchThemeCommand { get; }
        ICommand SwitchTuneInStateCommand { get; }

        Task InitializeAsync();
    }
}
