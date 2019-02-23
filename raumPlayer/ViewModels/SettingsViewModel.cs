using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.PrismEvents;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.UI.Text;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.System;

namespace raumPlayer.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INavigationService navigationService;
        private readonly IRaumFeldService raumFeldService;

        private Preset selectedPreset;
        public Preset SelectedPreset
        {
            get { return selectedPreset; }

            set { SetProperty(ref selectedPreset, value, () => {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["SELECTED_PRESET"] = value.Id;
            }); }
        }

        private ElementTheme elementTheme = ThemeSelectorService.Theme;
        public ElementTheme ElementTheme
        {
            get { return elementTheme; }

            set { SetProperty(ref elementTheme, value); }
        }

        private string versionDescription;
        public string VersionDescription
        {
            get { return versionDescription; }

            set { SetProperty(ref versionDescription, value); }
        }

        private bool isTuneInAvailable;
        public bool IsTuneInAvailable
        {
            get { return isTuneInAvailable; }
            set { SetProperty(ref isTuneInAvailable, value); }
        }

        private bool isCheckedMyMusic;
        public bool IsCheckedMyMusic
        {
            get { return isCheckedMyMusic; }
            set { SetProperty(ref isCheckedMyMusic, value); }
        }

        private bool isCheckedFavorites;
        public bool IsCheckedFavorites
        {
            get { return isCheckedFavorites; }
            set { SetProperty(ref isCheckedFavorites, value); }
        }

        private ObservableCollection<Preset> presets;
        public ObservableCollection<Preset> Presets
        {
            get { return presets; }
            set { SetProperty(ref presets, value); }
        }

        public string AppName => messagingService.AppDisplayName;
        public string AppPublisher => messagingService.AppPublisher;
        public string AppVersion => messagingService.AppVersion;
        public Uri AppLogo => messagingService.AppLogo;

        #region ICommands

        private ICommand switchThemeCommand;
        public ICommand SwitchThemeCommand
        {
            get
            {
                if (switchThemeCommand == null)
                {
                    switchThemeCommand = new DelegateCommand<object>(
                        async (param) =>
                        {
                            ElementTheme = (ElementTheme)param;
                            await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
                        });
                }

                return switchThemeCommand;
            }
        }

        private ICommand switchTuneInStateCommand;
        public ICommand SwitchTuneInStateCommand
        {
            get
            {
                if (switchTuneInStateCommand == null)
                {
                    switchTuneInStateCommand = new DelegateCommand<ToggleSwitch>(SetTuneInState);
                }

                return switchTuneInStateCommand;
            }
        }
        private async void SetTuneInState(ToggleSwitch e)
        {
            await raumFeldService.SetTuneInState(e.IsOn);
        }

        /// <summary>
        /// Save Settings
        /// </summary>
        private ICommand saveNavSettingsCommand;
        public ICommand SaveNavSettingsCommand
        {
            get
            {
                if (saveNavSettingsCommand == null)
                {
                    saveNavSettingsCommand = new DelegateCommand<RadioButton>(SaveNavSettings);
                }

                return saveNavSettingsCommand;
            }
        }
        private void SaveNavSettings(RadioButton e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["NAVIGATION"] = e.Name;

            switch (e.Name)
            {
                case "radioButtonMyMusic":
                    IsCheckedMyMusic = true;
                    break;
                case "radioButtonFavorites":
                    IsCheckedFavorites = true;
                    break;
                default:
                    break;
            }
        }

        #endregion

        public SettingsViewModel(INavigationService navigationServiceInstance, IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IRaumFeldService raumFeldServiceInstance)
        {
            navigationService = navigationServiceInstance;

            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            raumFeldService = raumFeldServiceInstance;

            eventAggregator.GetEvent<SystemUpdateIDChangedEvent>().Subscribe(onSystemUpdateIDChanged, ThreadOption.UIThread);

            Presets = new ObservableCollection<Preset>();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            VersionDescription = GetVersionDescription();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("NAVIGATION", out object obj))
            {
                switch ((string)obj)
                {
                    case "radioButtonMyMusic":
                        IsCheckedMyMusic = true;
                        break;
                    case "radioButtonFavorites":
                        IsCheckedFavorites = true;
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("_Settings_LoadPresets".GetLocalized()));
                string dataString = await FileIO.ReadTextAsync(file);

                Presets.Clear();

                foreach (Preset p in JsonConvert.DeserializeObject<List<Preset>>(dataString))
                {
                    Presets.Add(p);
                }

                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values.TryGetValue("SELECTED_PRESET", out object obj))
                {
                    SelectedPreset = Presets.Where(p => p.Id == (string)(obj)).FirstOrDefault();
                }
                else
                {
                    SelectedPreset = Presets.Where(p => p.Id == "Default").FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
        }

        public async void rtb_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RichEditBox textbox)
                {
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(((string)textbox.Tag).GetLocalized()));
                    if (file != null)
                    {

                        Windows.Storage.Streams.IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);

                        // Load the file into the Document property of the RichEditBox.
                        textbox.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);

                        textbox.Document.Selection.SetRange(0, int.MaxValue);
                        ITextSelection selectedText = textbox.Document.Selection;
                        if (selectedText != null && textbox.Foreground is SolidColorBrush color)
                        {
                            selectedText.CharacterFormat.ForegroundColor = color.Color;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Rate app; launches MS-Store to rate app
        /// </summary>
        public async void RateApp()
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store:REVIEW?PFN={Package.Current.Id.FamilyName}"));
        }

        /// <summary>
        /// Join Skype group; launches skype to join group
        /// </summary>
        public async void AddSkypeGroup()
        {
            await Launcher.LaunchUriAsync(new Uri("https://join.skype.com/l4wfW2nS2Vh6"));
        }

        public async void HowTo()
        {
            //var dialog = new HowTo();
            //await dialog.ShowAsync();
        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{package.DisplayName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void onSystemUpdateIDChanged(RaumFeldEvent args)
        {
            IsTuneInAvailable = await raumFeldService.GetTuneInState();
        }
    }
}
