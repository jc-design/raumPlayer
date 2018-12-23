using System;
using System.Collections.Generic;
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
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly INavigationService navigationService;
        private readonly IRaumFeldService raumFeldService;

        private NavigationView navigationView;

        public ICommand ItemInvokedCommand { get; }

        public ShellViewModel(INavigationService navigationServiceInstance, IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IRaumFeldService raumFeldServiceInstance)
        {
            navigationService = navigationServiceInstance;

            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            raumFeldService = raumFeldServiceInstance;

            eventAggregator.GetEvent<SystemUpdateIDChangedEvent>().Subscribe(onSystemUpdateIDChanged, ThreadOption.UIThread);
            eventAggregator.GetEvent<RaumFerldZonesLoadedEvent>().Subscribe(onRaumFeldZonesLoaded, ThreadOption.UIThread);

            ZoneViewModels = new ObservableCollection<ZoneViewModel>();

            ItemInvokedCommand = new DelegateCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked);
        }

        private bool isBackEnabled;
        public bool IsBackEnabled
        {
            get { return isBackEnabled; }
            set { SetProperty(ref isBackEnabled, value); }
        }

        private NavigationViewItem selected;
        public NavigationViewItem Selected
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

        private bool isTuneInAvailable = false;
        public bool IsTuneInAvailable
        {
            get { return isTuneInAvailable; }
            set { SetProperty(ref isTuneInAvailable, value); }
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

        public void Initialize(Frame frame, NavigationView navigationView)
        {
            this.navigationView = navigationView;
            frame.Navigated += frame_Navigated;
            this.navigationView.BackRequested += OnBackRequested;
        }

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            navigationService.GoBack();
        }

        private void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                navigationService.Navigate("Settings", null);
                return;
            }

            var item = navigationView.MenuItems
                            .OfType<NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            navigationService.Navigate(pageKey, null);
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = navigationService.CanGoBack();
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = navigationView.SettingsItem as NavigationViewItem;
                return;
            }

            Selected = navigationView.MenuItems
                            .OfType<NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
        {
            var sourcePageKey = sourcePageType.Name;
            sourcePageKey = sourcePageKey.Substring(0, sourcePageKey.Length - 4);
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == sourcePageKey;
        }

        private async void onSystemUpdateIDChanged(RaumFeldEvent args)
        {
            IsTuneInAvailable = await raumFeldService.GetTuneInState();
        }

        private async void onRaumFeldZonesLoaded(IList<ZoneViewModel> zones)
        {
            string zoneUdn = string.Empty;
            zoneUdn = await ApplicationData.Current.LocalSettings.ReadAsync<string>("ACTIVEZONE");
            zoneUdn = zones.Select(z => z).Where(z => z.Udn == zoneUdn).FirstOrDefault()?.Udn;
            if (string.IsNullOrEmpty(zoneUdn))
            {
                zoneUdn = zones.First().Udn;
                await ApplicationData.Current.LocalSettings.SaveAsync("ACTIVEZONE", zoneUdn);
            }

            ZoneViewModels.Clear();
            foreach (var zone in zones)
            {
                if (zone.Udn == zoneUdn)
                {
                    zone.IsActive = true;
                    if (navigationView.MenuItems[7] is NavigationViewItem item)
                    {
                        item.Content = zone.Name;
                    }
                    ActiveZoneViewModel = zone;
                }
                else { zone.IsActive = false; }

                ZoneViewModels.Add(zone);
            }
        }

        //public async Task SetActiveZoneViewModel(string zoneUdn)
        //{
        //    if (string.IsNullOrEmpty(zoneUdn))
        //    {
        //        zoneUdn = await ApplicationData.Current.LocalSettings.ReadAsync<string>("ACTIVEZONE");
        //        zoneUdn = ZoneViewModels.Select(z => z).Where(z => z.Udn == zoneUdn).FirstOrDefault()?.Udn;
        //        if (string.IsNullOrEmpty(zoneUdn))
        //        {
        //            zoneUdn = ZoneViewModels.First().Udn;
        //            await ApplicationData.Current.LocalSettings.SaveAsync("ACTIVEZONE", zoneUdn);
        //        }
        //    }

        //    foreach (var zone in ZoneViewModels)
        //    {
        //        if (zone.Udn == zoneUdn)
        //        {
        //            zone.IsActive = true;
        //            if (navigationView.MenuItems[7] is NavigationViewItem item)
        //            {
        //                item.Content = zone.Name;
        //            }
        //            ActiveZoneViewModel = zone;

        //            await ApplicationData.Current.LocalSettings.SaveAsync("ACTIVEZONE", zone.Udn);
        //        }
        //        else { zone.IsActive = false; }
        //    }
        //}
    }
}
