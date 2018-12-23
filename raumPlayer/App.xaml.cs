using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;

using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;

using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.Services;
using raumPlayer.ViewModels;
using raumPlayer.Views;
using Upnp;

using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace raumPlayer
{
    public sealed partial class App : PrismUnityApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void ConfigureContainer()
        {
            // register a singleton using Container.RegisterType<IInterface, Type>(new ContainerControlledLifetimeManager());
            base.ConfigureContainer();
            Container.RegisterType<IWhatsNewDisplayService, WhatsNewDisplayService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFirstRunDisplayService, FirstRunDisplayService>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            //Container.RegisterType<ILoggingService, LoggingService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMessagingService, MessagingService>(new ContainerControlledLifetimeManager());

            Container.RegisterType<INetWorkDeviceWatcher, NetWorkDeviceWatcher>(new ContainerControlledLifetimeManager());
            Container.RegisterType<INetWorkSocketListener, NetWorkSocketListener>(new ContainerControlledLifetimeManager());
            Container.RegisterType<INetWorkSubscriber, NetWorkSubscriber>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IRaumFeldService, RaumFeldService>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IShellViewModel, ShellViewModel>(new ContainerControlledLifetimeManager());
            //Container.RegisterType<IShellNavigationItem, ShellNavigationViewModel>();
            //Container.RegisterType<IShellNavigationItem, TuneInNavigationViewModel>();
            //Container.RegisterType<IShellNavigationItem, ManageZonesNavigationViewModel>();

            Container.RegisterType<IMediaDevice, MediaServer>();
            Container.RegisterType<IMediaDevice, MediaRenderer>();

            Container.RegisterType<IBrowseViewModel, BrowseViewModel>();
            Container.RegisterType<IPivotItemViewModel, PivotItemViewModel>();
            Container.RegisterType<IZoneViewModel, ZoneViewModel>();
            Container.RegisterType<IRoomViewModel, RoomViewModel>();
            Container.RegisterType<ISettingsViewModel, SettingsViewModel>();

            Container.RegisterType<DIDLContainer>();
            Container.RegisterType<DIDLItem>();
            Container.RegisterType<ElementBase, ElementBase>();
            Container.RegisterType<ElementBase, ElementItem>();
            Container.RegisterType<ElementBase, ElementContainer>();
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            return launchApplicationAsync(PageTokens.SettingsPage, null);
        }

        private async Task launchApplicationAsync(string page, object launchParam)
        {
            // For Desktop
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    {
                        titleBar.ButtonBackgroundColor = Colors.Transparent;
                        titleBar.ButtonForegroundColor = (Color)this.Resources["SystemAccentColor"];
                        titleBar.ForegroundColor = (Color)this.Resources["SystemAccentColor"];
                        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                        titleBar.ButtonInactiveForegroundColor = (Color)this.Resources["SystemAccentColor"];
                    }
                }

                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
            }

            Interfaces.ThemeSelectorService.SetRequestedTheme();
            NavigationService.Navigate(page, launchParam);
            Window.Current.Activate();
            await Container.Resolve<IWhatsNewDisplayService>().ShowIfAppropriateAsync();
            await Container.Resolve<IFirstRunDisplayService>().ShowIfAppropriateAsync();

            await Container.Resolve<INetWorkSocketListener>().StartListening(22110);
            Container.Resolve<INetWorkDeviceWatcher>().StartDeviceWatcher();
            await Container.Resolve<IRaumFeldService>().InitializeAsync();

            await Container.Resolve<ISettingsViewModel>().InitializeAsync();

            await Task.CompletedTask;
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ToastNotification && args.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                // Handle a toast notification here
                // Since dev center, toast, and Azure notification hub will all active with an ActivationKind.ToastNotification
                // you may have to parse the toast data to determine where it came from and what action you want to take
                // If the app isn't running then launch the app here
                OnLaunchApplicationAsync(args as LaunchActivatedEventArgs);
            }

            return Task.CompletedTask;
        }

        //protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        //{
        //    base.OnBackgroundActivated(args);
        //    CreateAndConfigureContainer();
        //}

        protected override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);

            // We are remapping the default ViewNamePage and ViewNamePageViewModel naming to ViewNamePage and ViewNameViewModel to
            // gain better code reuse with other frameworks and pages within Windows Template Studio
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "raumPlayer.ViewModels.{0}ViewModel, raumPlayer", viewType.Name.Substring(0, viewType.Name.Length - 4));
                return Type.GetType(viewModelTypeName);
            });
            await base.OnInitializeAsync(args);
        }

        protected async override Task OnResumeApplicationAsync(IActivatedEventArgs args)
        {
            await Container.Resolve<INetWorkSocketListener>().StartListening(22110);
            Container.Resolve<INetWorkDeviceWatcher>().StartDeviceWatcher();

            await base.OnResumeApplicationAsync(args);
        }

        protected override Task OnSuspendingApplicationAsync()
        {
            Container.Resolve<INetWorkDeviceWatcher>().StopDeviceWatcher();
            Container.Resolve<INetWorkSocketListener>().StopListening();

            return base.OnSuspendingApplicationAsync();
        }

        public void SetNavigationFrame(Frame frame)
        {
            var sessionStateService = Container.Resolve<ISessionStateService>();
            CreateNavigationService(new FrameFacadeAdapter(frame), sessionStateService);
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<ShellPage>();
            shell.SetRootFrame(rootFrame);
            return shell;
        }

        protected override IDeviceGestureService OnCreateDeviceGestureService()
        {
            var service = base.OnCreateDeviceGestureService();
            service.UseTitleBarBackButton = false;
            return service;
        }
    }
}
