using JCDesign.Helper;
using JCDesign.Services;
using JCDesign.UI;
using roomZone.Helpers;
using roomZone.Models;
using roomZone.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace roomZone.ViewModels
{
    public class ManageZonesViewModel : ViewModelBase
    {
        #region Private Properties

        #endregion

        #region Public Properties

        private ObservableCollection<ZoneViewModel> zoneViewModels = new ObservableCollection<ZoneViewModel>();
        public ObservableCollection<ZoneViewModel> ZoneViewModels
        {
            get { return zoneViewModels; }
            set { Set(ref zoneViewModels, value); }
        }

        #endregion

        #region ICommands

        /// <summary>
        /// Setup FlyoutMenu
        /// </summary>
        private ICommand initializeMenuFlyoutCommand;
        public ICommand InitializeMenuFlyoutCommand
        {
            get
            {
                if (initializeMenuFlyoutCommand == null)
                {
                    initializeMenuFlyoutCommand = new RelayCommand<Button>(InitializeMenuFlyout);
                }

                return initializeMenuFlyoutCommand;
            }
        }
        public void InitializeMenuFlyout(Button e)
        {
            if (e.DataContext is Room room)
            {
                if (e.Flyout is MenuFlyout flyout)
                {
                    flyout.Items.Clear();

                    foreach (var zone in ZoneViewModels)
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem
                        {
                            Text = zone.Name
                        };
                        if (room.ViewModel.Udn == zone.Udn) { item.IsEnabled = false; }
                        else
                        {
                            item.IsEnabled = true;
                            DragObject d = new DragObject() { RoomUdn = room.Udn, RoomName = room.Name, ZoneUdn = zone.Udn, ZoneName = zone.Name };
                            item.Tag = d;

                            item.Tapped += Item_Tapped;
                        }
                        flyout.Items.Add(item);
                    }

                    MenuFlyoutItem newZoneItem = new MenuFlyoutItem
                    {
                        Text = UIService.GetResource("NewZone"),
                        IsEnabled = true,
                        Tag = new DragObject() { RoomUdn = room.Udn, RoomName = room.Name, ZoneUdn = App.UpnpBaseUnit.CreateNewGuid(), ZoneName = UIService.GetResource("NewZone") },

                    };
                    newZoneItem.Tapped += Item_Tapped;
                    flyout.Items.Add(newZoneItem);
                }
            }
        }

        private async void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                if (item.Tag is DragObject d)
                {
                    bool returnValue = await App.UpnpBaseUnit.ConnectRoomToZone(d.RoomUdn, d.ZoneUdn);
                    if (!returnValue) { Shell.Instance.ViewModel.ZoneItems.Last().IsZone = false; }
                }
            }
        }

        //private async void zoneGrid_DragOver(object sender, DragEventArgs e)
        //{
        //    if (sender is FrameworkElement fe)
        //    {
        //        //if (fe.DataContext is ZoneViewModel vm)
        //        //{
        //        var deferral = e.GetDeferral();

        //        e.AcceptedOperation = DataPackageOperation.Move;
        //        //e.DragUIOverride.SetContentFromBitmapImage(null);                                             // Sets a custom glyph
        //        e.DragUIOverride.IsCaptionVisible = true;                                                       // Sets if the caption is visible
        //        e.DragUIOverride.IsContentVisible = false;                                                      // Sets if the dragged content is visible
        //        e.DragUIOverride.IsGlyphVisible = true;                                                         // Sets if the glyph is visibile

        //        DragObject drag = await Json.ToObjectAsync<DragObject>(await e.DataView.GetTextAsync());

        //        e.DragUIOverride.Caption = string.Format("{0} {1}", UIService.GetResource("Move"), drag.Name);   // Sets custom UI text

        //        deferral.Complete();
        //        //}
        //    }
        //}

        //private async void zoneGrid_Drop(object sender, DragEventArgs e)
        //{
        //    if (sender is FrameworkElement fe)
        //    {
        //        var deferral = e.GetDeferral();

        //        DragObject drag = await Json.ToObjectAsync<DragObject>(await e.DataView.GetTextAsync());

        //        //Switch Zone
        //        if (fe.DataContext is ZoneViewModel vm)
        //        {
        //            // Only start method when dropping on new zone
        //            if (vm.Udn != drag.ZoneUdn)
        //            {
        //                await ViewModel.ZoneItems.Last().ConnectRoomToZone(drag.Udn, vm.Udn);
        //            }
        //        }
        //        // Create new Zone
        //        else
        //        {
        //            await ViewModel.ZoneItems.Last().ConnectRoomToZone(drag.Udn, RaumfeldBaseUnit.Instance.CreateNewGuid());
        //        }

        //        deferral.Complete();
        //    }
        //}

        //private async void roomGrid_DragStarting(UIElement sender, DragStartingEventArgs args)
        //{
        //    if (sender is FrameworkElement fe)
        //    {
        //        if (fe.DataContext is Room room)
        //        {
        //            var deferral = args.GetDeferral();

        //            DragObject drag = new DragObject() { Udn = room.Udn, Name = room.Name, ZoneUdn = room.ViewModel.Udn };

        //            var uielement = XamlHelper.FindChildren<TextBlock>(fe, t => (t?.Tag as string ?? "") == "room").FirstOrDefault();

        //            if (uielement != null)
        //            {
        //                // Generate a bitmap with only the TextBox
        //                // We need to take the deferral as the rendering won't be completed synchronously

        //                var rtb = new RenderTargetBitmap();
        //                await rtb.RenderAsync(uielement);
        //                var buffer = await rtb.GetPixelsAsync();
        //                var bitmap = SoftwareBitmap.CreateCopyFromBuffer(buffer,
        //                    BitmapPixelFormat.Bgra8,
        //                    rtb.PixelWidth,
        //                    rtb.PixelHeight,
        //                    BitmapAlphaMode.Premultiplied);
        //                args.DragUI.SetContentFromSoftwareBitmap(bitmap, new Point(0, 0));
        //                args.Data.RequestedOperation = DataPackageOperation.Move;
        //                args.Data.SetText(await Json.StringifyAsync(drag));
        //            }

        //            deferral.Complete();
        //        }
        //    }
        //}

        /// <summary>
        /// Set new active Zone
        /// </summary>
        private ICommand checkedCommand;
        public ICommand CheckedCommand
        {
            get
            {
                if (checkedCommand == null)
                {
                    checkedCommand = new RelayCommand<ZoneViewModel>(Checked);
                }

                return checkedCommand;
            }
        }
        public void Checked(ZoneViewModel obj)
        {
            if (obj != null) { obj.IsActive = true; }
        }

        #endregion

        #region events

        public async void OnZonesFound(object sender, EventArgs<ObservableCollection<ZoneViewModel>> e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () => {

                if ((ZoneViewModels?.Count() ?? 0) > 0)
                {
                    ZoneViewModels.Clear();
                }

                //Get saved settings
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string udn = localSettings.Values["ACTIVEZONE"] as string;

                //Check is there were settings
                //If not, get UDN from active Renderer 
                if (string.IsNullOrEmpty(udn)) { udn = PivotItemViewModel.ActiveRenderer?.UDN ?? string.Empty; }
                bool notChecked = true;
                ZoneViewModel zone = null;

                if ((e?.Value?.Count() ?? 0) > 0)
                {
                    foreach (var vm in e.Value)
                    {
                        vm.IsActive = false;
                        

                        //Save pointer to previous selected zone
                        if (notChecked)
                        {
                            zone = vm;
                            if (udn == vm.Udn) { notChecked = false; }
                        }

                        ZoneViewModels.Add(vm);
                    }

                    if (zone.MediaRenderer != null) { zone.IsActive = true; }

                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    foreach (var vm in ZoneViewModels)
                    {
                        await vm.InitVolumes();
                    }
                }
            });
        }

        #endregion

        public ManageZonesViewModel()
        {
        }

        public async Task InitViewModel()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () => {

                ZoneViewModels.Clear();
                foreach (var vm in Shell.Instance.ViewModel.ZoneViewModels)
                {
                    ZoneViewModels.Add(vm);
                    await vm.InitVolumes();
                }
            });

            App.UpnpBaseUnit.ZoneLoaded += OnZonesFound;
        }

        public void DeInitViewModel()
        {
            App.UpnpBaseUnit.ZoneLoaded -= OnZonesFound;
        }
    }
}
