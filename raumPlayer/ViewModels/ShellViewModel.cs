using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Unity.Windows;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.PrismEvents;
using raumPlayer.Views;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace raumPlayer.ViewModels
{
    public class ShellViewModel : ViewModelBase, IShellViewModel
    {
        private const string PanoramicStateName = "WideState";
        private const string WideStateName = "NormalState";
        private const string NarrowStateName = "NarrowState";

        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INavigationService navigationService;

        public ShellViewModel(INavigationService navigationServiceInstance, IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance)
        {
            navigationService = navigationServiceInstance;

            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;

            ZoneViewModels = new ObservableCollection<ZoneViewModel>();

            StateChangedCommand = new DelegateCommand<VisualStateChangedEventArgs>(args => goToState(args.NewState.Name));
        }

        private bool isPaneOpen;
        public bool IsPaneOpen
        {
            get { return isPaneOpen; }
            set { SetProperty(ref isPaneOpen, value); }
        }

        private object selected;
        public object Selected
        {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }

        private string header = string.Empty;
        public string Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value, () =>
                {
                    BusyVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        private Visibility busyVisibility = Visibility.Visible;
        public Visibility BusyVisibility
        {
            get { return busyVisibility; }
            set { SetProperty(ref busyVisibility, value); }
        }

        private SplitViewDisplayMode displayMode = SplitViewDisplayMode.CompactInline;
        public SplitViewDisplayMode DisplayMode
        {
            get { return displayMode; }
            set { SetProperty(ref displayMode, value); }
        }

        private object lastSelectedItem;

        private ObservableCollection<IShellNavigationItem> primaryItems = new ObservableCollection<IShellNavigationItem>();
        public ObservableCollection<IShellNavigationItem> PrimaryItems
        {
            get { return primaryItems; }
            set { SetProperty(ref primaryItems, value); }
        }

        private ObservableCollection<IShellNavigationItem> secondaryItems = new ObservableCollection<IShellNavigationItem>();
        public ObservableCollection<IShellNavigationItem> SecondaryItems
        {
            get { return secondaryItems; }
            set { SetProperty(ref secondaryItems, value); }
        }

        private ObservableCollection<IShellNavigationItem> otherItems = new ObservableCollection<IShellNavigationItem>();
        public ObservableCollection<IShellNavigationItem> OtherItems
        {
            get { return otherItems; }
            set { SetProperty(ref otherItems, value); }
        }

        private ZoneViewModel activeZoneViewModel;
        public ZoneViewModel ActiveZoneViewModel
        {
            get { return activeZoneViewModel; }
            set { SetProperty(ref activeZoneViewModel, value); }
        }

        private ObservableCollection<ZoneViewModel> zoneViewModels;
        public ObservableCollection<ZoneViewModel> ZoneViewModels
        {
            get { return zoneViewModels; }
            set { SetProperty(ref zoneViewModels, value); }
        }

        //private SolidColorBrush backgroundColor;
        //public SolidColorBrush BackgroundColor
        //{
        //    get { return backgroundColor; }
        //    set { SetProperty(ref backgroundColor, value); }
        //}

        //private BitmapImage albumArtImage;
        //public BitmapImage AlbumArtImage
        //{
        //    get { return albumArtImage; }
        //    set { SetProperty(ref albumArtImage, value); }
        //}

        /// <summary>
        /// Navigate to selected page
        /// </summary>
        private ICommand itemSelectedCommand;
        public ICommand ItemSelectedCommand
        {
            get
            {
                if (itemSelectedCommand == null)
                {
                    itemSelectedCommand = new DelegateCommand<IShellNavigationItem>(ItemSelected);
                }

                return itemSelectedCommand;
            }
        }
        public void ItemSelected(IShellNavigationItem e)
        {
            if (DisplayMode == SplitViewDisplayMode.CompactOverlay || DisplayMode == SplitViewDisplayMode.Overlay || Window.Current.Bounds.Width < (double)PrismUnityApplication.Current.Resources["NormalMinWidth"])
            {
                IsPaneOpen = false;
            }

            navigate(e);
        }

        private ICommand secondaryItemSelectedCommand;
        public ICommand SecondaryItemSelectedCommand
        {
            get
            {
                if (secondaryItemSelectedCommand == null)
                {
                    secondaryItemSelectedCommand = new DelegateCommand<IShellNavigationItem>(SecondaryItemSelected);
                }

                return itemSelectedCommand;
            }
        }
        public void SecondaryItemSelected(IShellNavigationItem e)
        {
            if (DisplayMode == SplitViewDisplayMode.CompactOverlay || DisplayMode == SplitViewDisplayMode.Overlay || Window.Current.Bounds.Width < (double)PrismUnityApplication.Current.Resources["NormalMinWidth"])
            {
                IsPaneOpen = false;
            }

            navigateSecondary(e);
        }

        private ICommand openPaneCommand;
        public ICommand OpenPaneCommand
        {
            get
            {
                if (openPaneCommand == null)
                {
                    openPaneCommand = new DelegateCommand(() => IsPaneOpen = !IsPaneOpen);
                }

                return openPaneCommand;
            }
        }
        public ICommand StateChangedCommand { get; }

        public void Initialize(Frame frame)
        {
            frame.Navigated += frame_Navigated;
            populateNavItems();
            initializeState(Window.Current.Bounds.Width);
        }

        public async Task SetActiveZoneViewModel(string zoneUdn)
        {
            if (string.IsNullOrEmpty(zoneUdn))
            {
                zoneUdn = await ApplicationData.Current.LocalSettings.ReadAsync<string>("ACTIVEZONE");
                zoneUdn = ZoneViewModels.Select(z => z).Where(z => z.Udn == zoneUdn).FirstOrDefault()?.Udn;
                if (string.IsNullOrEmpty(zoneUdn))
                {
                    zoneUdn = ZoneViewModels.First().Udn;
                    await ApplicationData.Current.LocalSettings.SaveAsync("ACTIVEZONE", zoneUdn);
                }
            }

            foreach (var zone in ZoneViewModels)
            {
                if (zone.Udn == zoneUdn)
                {
                    zone.IsActive = true;
                    (SecondaryItems.Select(i => i).Where(i => i.GetType() == typeof(ManageZonesNavigationViewModel)).FirstOrDefault()).Label = zone.Name;
                    ActiveZoneViewModel = zone;

                    await ApplicationData.Current.LocalSettings.SaveAsync("ACTIVEZONE", zone.Udn);
                }
                else { zone.IsActive = false; }
            }
        }

        public void SetBackbuttonVisibility(bool state)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (state || navigationService.CanGoBack()) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void goToState(string stateName)
        {
            switch (stateName)
            {
                case PanoramicStateName:
                    DisplayMode = SplitViewDisplayMode.CompactInline;
                    break;
                case WideStateName:
                    DisplayMode = SplitViewDisplayMode.CompactInline;
                    IsPaneOpen = false;
                    break;
                case NarrowStateName:
                    DisplayMode = SplitViewDisplayMode.Overlay;
                    IsPaneOpen = false;
                    break;
                default:
                    break;
            }
        }

        private void initializeState(double windowWith)
        {
            if (windowWith < (double)PrismUnityApplication.Current.Resources["NormalMinWidth"])
            {
                goToState(NarrowStateName);
            }
            else if (windowWith < (double)PrismUnityApplication.Current.Resources["WideMinWidth"])
            {
                goToState(WideStateName);
            }
            else
            {
                goToState(PanoramicStateName);
            }
        }

        private void populateNavItems()
        {
            primaryItems.Clear();
            secondaryItems.Clear();

            // TODO WTS: Change the symbols for each item as appropriate for your app
            // More on Segoe UI Symbol icons: https://docs.microsoft.com/windows/uwp/style/segoe-ui-symbol-font
            // Or to use an IconElement instead of a Symbol see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/projectTypes/navigationpane.md
            // Edit String/en-US/Resources.resw: Add a menu item title for each page

            PrimaryItems.Add(PrismUnityApplication.Current.Container.Resolve<ShellNavigationViewModel>(new ResolverOverride[]
                {
                    new ParameterOverride("label", "MyMusic".GetLocalized()), new ParameterOverride("symbol", "\uE189"), new ParameterOverride("pageIdentifier", "MyMusic")
                }));
            PrimaryItems.Add(PrismUnityApplication.Current.Container.Resolve<ShellNavigationViewModel>(new ResolverOverride[]
                {
                    new ParameterOverride("label", "Playlists".GetLocalized()), new ParameterOverride("symbol", "\uE90B"), new ParameterOverride("pageIdentifier", "Playlist")
                }));
            PrimaryItems.Add(PrismUnityApplication.Current.Container.Resolve<ShellNavigationViewModel>(new ResolverOverride[]
                {
                    new ParameterOverride("label", "Favorites".GetLocalized()), new ParameterOverride("symbol", "\uEB51"), new ParameterOverride("pageIdentifier", "Favorite")
                }));
            PrimaryItems.Add(PrismUnityApplication.Current.Container.Resolve<TuneInNavigationViewModel>(new ResolverOverride[]
                {
                    new ParameterOverride("label", "TuneIn".GetLocalized()), new ParameterOverride("symbol", "\uEC05"), new ParameterOverride("pageIdentifier", "Radio")
                }));

            SecondaryItems.Add(PrismUnityApplication.Current.Container.Resolve<ManageZonesNavigationViewModel>(new ResolverOverride[]
                {
                    new ParameterOverride("symbol", "\uE965"), new ParameterOverride("pageIdentifier", "Zone"), new ParameterOverride("secondPageIdentifier", "ManageZones")
                }));

            OtherItems.Add(PrismUnityApplication.Current.Container.Resolve<ShellNavigationViewModel>(new ResolverOverride[]
                {
                     new ParameterOverride("label", "Settings".GetLocalized()), new ParameterOverride("symbol", "\uE115"), new ParameterOverride("pageIdentifier", "Settings")
                }));

        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e != null)
            {
                var vm = e.SourcePageType.ToString().Split('.').Last().Replace("Page", string.Empty);
                var navigationItem = PrimaryItems?.FirstOrDefault(i => i.PageIdentifier == vm);
                if (navigationItem == null)
                {
                    navigationItem = SecondaryItems?.FirstOrDefault(i => i.PageIdentifier == vm);
                }

                if (navigationItem == null)
                {
                    navigationItem = OtherItems?.FirstOrDefault(i => i.PageIdentifier == vm);
                }

                if (navigationItem != null)
                {
                    changeSelected(lastSelectedItem, navigationItem);
                    lastSelectedItem = navigationItem;

                    Header = navigationItem.Label;
                }
            }
        }

        private void changeSelected(object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                (oldValue as ShellNavigationViewModel).IsSelected = false;
            }

            if (newValue != null)
            {
                (newValue as ShellNavigationViewModel).IsSelected = true;
                Selected = newValue;
            }
        }

        private void navigate(object item)
        {
            if (item is IShellNavigationItem navigationItem)
            {
                navigationService.Navigate(navigationItem.PageIdentifier, null);
            }
        }

        private void navigateSecondary(object item)
        {
            if (item is IShellNavigationItem navigationItem)
            {
                navigationService.Navigate(navigationItem.SecondPageIdentifier, null);
            }
        }

        //private ICommand getAlbumArtCommand;
        //public ICommand GetAlbumArtCommand
        //{
        //    get
        //    {
        //        if (getAlbumArtCommand == null)
        //        {
        //            getAlbumArtCommand = new DelegateCommand<ZoneViewModel>(async (param) =>
        //            {
        //                BackgroundColor = param.CurrentTrackMetaData.AverageColorImageArt;
        //                AlbumArtImage = param.CurrentTrackMetaData.ImageArt;

        //                //try
        //                //{
        //                //    string url;

        //                //    if (string.IsNullOrEmpty(param.AlbumArtUri))
        //                //    {
        //                //        // Alternative UrlBitmap if string is NullOrEmpty
        //                //        url = "ms-appx:///Assets/disc_gray.png";
        //                //    }
        //                //    else { url = param.AlbumArtUri; }

        //                //    BitmapImage bitmapImage = new BitmapImage(new Uri(url, UriKind.Absolute));

        //                //    RandomAccessStreamReference randomAccessStream = RandomAccessStreamReference.CreateFromUri(bitmapImage.UriSour‌​ce);

        //                //    using (IRandomAccessStream stream = await randomAccessStream.OpenReadAsync())
        //                //    {
        //                //        //Create a decoder for the image
        //                //        var decoder = await BitmapDecoder.CreateAsync(stream);

        //                //        var pixels = await decoder.GetPixelDataAsync(
        //                //                             BitmapPixelFormat.Rgba8,
        //                //                             BitmapAlphaMode.Ignore,
        //                //                             new BitmapTransform { ScaledHeight = 50, ScaledWidth = 50 },
        //                //                             ExifOrientationMode.IgnoreExifOrientation,
        //                //                             ColorManagementMode.DoNotColorManage);

        //                //        //Get the bytes of the 1x1 scaled image
        //                //        var bytes = pixels.DetachPixelData();

        //                //        long[] totals = new long[] { 0, 0, 0 };

        //                //        for (int i = 0; i < bytes.Length; i += 4)
        //                //        {
        //                //            totals[0] += bytes[i + 0];
        //                //            totals[1] += bytes[i + 1];
        //                //            totals[2] += bytes[i + 2];
        //                //        }

        //                //        int avgR = (int)(totals[0] / (bytes.Length / 4));
        //                //        int avgG = (int)(totals[1] / (bytes.Length / 4));
        //                //        int avgB = (int)(totals[2] / (bytes.Length / 4));

        //                //        BackgroundColor = new SolidColorBrush(Color.FromArgb((byte)127, (byte)avgR, (byte)avgG, (byte)avgB));
        //                //        AlbumArtImage = bitmapImage;
        //                //    }
        //                //}
        //                //catch (Exception)
        //                //{
        //                //    BackgroundColor = new SolidColorBrush(Colors.Transparent);
        //                //    AlbumArtImage = new BitmapImage(new Uri("ms-appx:///Assets/disc_gray.png", UriKind.Absolute));
        //                //}
        //            },

        //            (param) => param == ActiveZoneViewModel);
        //        }

        //        return getAlbumArtCommand;
        //    }
        //}
    }
}
