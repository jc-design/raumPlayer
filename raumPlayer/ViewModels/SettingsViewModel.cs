using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;
using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.PrismEvents;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace raumPlayer.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INavigationService navigationService;
        private readonly IRaumFeldService raumFeldService;

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

        private bool isTuneInAvailable = false;
        public bool IsTuneInAvailable
        {
            get { return isTuneInAvailable; }
            set { SetProperty(ref isTuneInAvailable, value); }
        }

        private bool isCheckedMyMusic = false;
        public bool IsCheckedMyMusic
        {
            get { return isCheckedMyMusic; }
            set { SetProperty(ref isCheckedMyMusic, value); }
        }

        private bool isCheckedFavorites = false;
        public bool IsCheckedFavorites
        {
            get { return isCheckedFavorites; }
            set { SetProperty(ref isCheckedFavorites, value); }
        }

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
        }

        #endregion


        public SettingsViewModel(INavigationService navigationServiceInstance, IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IRaumFeldService raumFeldServiceInstance)
        {
            navigationService = navigationServiceInstance;

            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            raumFeldService = raumFeldServiceInstance;

            eventAggregator.GetEvent<SystemUpdateIDChangedEvent>().Subscribe(onSystemUpdateIDChanged, ThreadOption.UIThread);
        }


        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            VersionDescription = GetVersionDescription();
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

        public async Task InitializeAsync()
        {
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

            IsTuneInAvailable = await raumFeldService.GetTuneInState();
        }
    }
}
