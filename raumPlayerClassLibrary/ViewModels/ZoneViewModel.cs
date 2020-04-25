using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Unity.Windows;
using Prism.Windows.Mvvm;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.PrismEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Upnp;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace raumPlayer.ViewModels
{
    public class ZoneViewModel : ViewModelBase, IZoneViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly ICachingService cachingService;
        private readonly IShellViewModel shellViewModel;
        private readonly IRaumFeldService raumFeldService;
        private readonly MediaRenderer zoneViewModelRenderer;

        private ICommand getZoneVolumeCommand;
        public ICommand GetZoneVolumeCommand
        {
            get
            {
                if (getZoneVolumeCommand == null)
                {
                    getZoneVolumeCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return ; }
                        ServiceActionReturnMessage message = await zoneViewModelRenderer.GetVolume();
                        if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is double volume)
                        {
                            ZoneVolume = volume;
                        }
                    });
                }
                return getZoneVolumeCommand;
            }
        }

        private ICommand getZoneMuteCommand;
        public ICommand GetZoneMuteCommand
        {
            get
            {
                if (getZoneMuteCommand == null)
                {
                    getZoneMuteCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        ServiceActionReturnMessage message = await zoneViewModelRenderer.GetMute();
                        if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is bool mute)
                        {
                            ZoneMute = mute;
                        }
                    });
                }
                return getZoneMuteCommand;
            }
        }

        private ICommand getCurrentTransportActionsCommand;
        public ICommand GetCurrentTransportActionsCommand
        {
            get
            {
                if (getCurrentTransportActionsCommand == null)
                {
                    getCurrentTransportActionsCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        ServiceActionReturnMessage message = await zoneViewModelRenderer.GetCurrentTransportActions();
                        if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is string[] value)
                        {
                            resetPlayState();
                            setPlayState(value);
                        }
                    });
                }
                return getCurrentTransportActionsCommand;
            }
        }

        private ICommand getTransportSettingsCommand;
        public ICommand GetTransportSettingsCommand
        {
            get
            {
                if (getTransportSettingsCommand == null)
                {
                    getTransportSettingsCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        ServiceActionReturnMessage message = await zoneViewModelRenderer.GetTransportSettings();
                        if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is string value)
                        {
                            resetPlayMode();
                            setPlayMode(value);
                        }
                    });
                }
                return getTransportSettingsCommand;
            }
        }

        private ICommand getMediaInfoCommand;
        public ICommand GetMediaInfoCommand
        {
            get
            {
                if (getMediaInfoCommand == null)
                {
                    getMediaInfoCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        ServiceActionReturnMessage messageMediaInfo = await zoneViewModelRenderer.GetMediaInfo();
                        if (messageMediaInfo.ActionStatus == ActionStatus.Okay && messageMediaInfo.ReturnValue is Dictionary<string, string> returnValueMediaInfo)
                        {

                            if (returnValueMediaInfo.TryGetValue("NrTracks", out string tracks)) { NumberOfTracks = Int32.Parse(tracks); }
                            else { CurrentTrack = 0; }

                            if (returnValueMediaInfo.TryGetValue("CurrentURIMetaData", out string tracksMetaData))
                            {
                                DIDLLite didl = tracksMetaData.Deserialize<DIDLLite>();
                                if ((didl?.Containers?.Count() ?? 0) != 0)
                                {
                                    if (await raumFeldService.BrowseChildren(ZoneViewModelTracks, didl.Containers?.First().Id, true))
                                    {
                                        setSelection(CurrentTrackMetaData);
                                    }
                                    else { ZoneViewModelTracks.Clear(); }
                                }
                            }
                        }
                    });
                }
                return getMediaInfoCommand;
            }
        }

        private ICommand getPositionInfoCommand;
        public ICommand GetPositionInfoCommand
        {
            get
            {
                if (getPositionInfoCommand == null)
                {
                    getPositionInfoCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        ServiceActionReturnMessage message = await zoneViewModelRenderer.GetPositionInfo();
                        if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is Dictionary<string, string> returnValuePositionInfo)
                        {
                            if (returnValuePositionInfo.TryGetValue("Track", out string track)) { CurrentTrack = Int32.Parse(track); }
                            else { CurrentTrack = 0; }

                            if (returnValuePositionInfo.TryGetValue("TrackDuration", out string trackDuration)) { CurrentTrackDuration = TimeSpan.Parse(trackDuration); }
                            else { CurrentTrackDuration = new TimeSpan(0); }

                            if (returnValuePositionInfo.TryGetValue("TrackMetaData", out string trackMetaData))
                            {
                                DIDLLite didl = trackMetaData.Deserialize<DIDLLite>();
                                if ((didl?.Items?.Count() ?? 0) != 0)
                                {
                                    //CurrentTrackMetaData = PrismUnityApplication.Current.Container.Resolve<ElementItem>(new ResolverOverride[]
                                    //    {
                                    //       new ParameterOverride("didl", didl?.Items?.FirstOrDefault())
                                    //    });
                                    //await cachingService.AddElement(CurrentTrackMetaData);
                                    CurrentTrackMetaData = PrismUnityApplication.Current.Container.Resolve<ElementBase>("DIDLItem",
                                        new DependencyOverride(typeof(IEventAggregator), eventAggregator),
                                        new DependencyOverride(typeof(ICachingService), cachingService),
                                        new DependencyOverride(typeof(DIDLItem), didl?.Items?.FirstOrDefault()));

                                    setSelection(CurrentTrackMetaData);
                                }
                                else { CurrentTrackMetaData = null; }
                            }
                            else { CurrentTrackMetaData = null; }

                            if (returnValuePositionInfo.TryGetValue("TrackURI", out string trackURI))
                            {
                                if (string.IsNullOrEmpty(trackURI)) { CurrentTrackURI = null; }
                                else { CurrentTrackURI = new Uri(trackURI); }
                            }
                            else { CurrentTrackURI = null; }
                        }
                    });
                }
                return getPositionInfoCommand;
            }
        }

        private ICommand setZoneVolumeCommand;
        public ICommand SetZoneVolumeCommand
        {
            get
            {
                if (setZoneVolumeCommand == null)
                {
                    setZoneVolumeCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        if (param is Slider slider)
                        {
                            if (ZoneVolume == slider.Value) { return; }

                            ZoneVolume = slider.Value;
                            ServiceActionReturnMessage message = await zoneViewModelRenderer.SetVolume(slider.ToString());
                        }
                    });
                }
                return setZoneVolumeCommand;
            }
        }

        private ICommand setZoneMuteCommand;
        public ICommand SetZoneMuteCommand
        {
            get
            {
                if (setZoneMuteCommand == null)
                {
                    setZoneMuteCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        if (param is CheckBox checkbox)
                        {
                            if (ZoneMute == checkbox.IsChecked) { return; }

                            ZoneMute = (checkbox?.IsChecked ?? false);
                            ServiceActionReturnMessage message = await zoneViewModelRenderer.SetMute((checkbox?.IsChecked ?? false) ? "1" : "0");
                        }
                    });
                }
                return setZoneMuteCommand;
            }
        }

        private ICommand setPlayModeCommand;
        public ICommand SetPlayModeCommand
        {
            get
            {
                if (setPlayModeCommand == null)
                {
                    setPlayModeCommand = new DelegateCommand<object>(async (param) =>
                    {
                        if (zoneViewModelRenderer == null) { return; }
                        if (param is string playmode)
                        {
                            await zoneViewModelRenderer.SetPlayMode(playmode);
                        }
                    });
                }
                return setPlayModeCommand;
            }
        }

        private ICommand previousCommand;
        public ICommand PreviousCommand
        {
            get
            {
                if (previousCommand == null)
                {
                    previousCommand = new DelegateCommand(async () =>
                    {
                        await zoneViewModelRenderer.Previous();
                    });
                }
                return previousCommand;
            }
        }

        private ICommand playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (playCommand == null)
                {
                    playCommand = new DelegateCommand(async () =>
                    {
                        await zoneViewModelRenderer.Play();
                    });
                }
                return playCommand;
            }
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (pauseCommand == null)
                {
                    pauseCommand = new DelegateCommand(async () =>
                    {
                        await zoneViewModelRenderer.Pause();
                    });
                }
                return pauseCommand;
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                {
                    stopCommand = new DelegateCommand(async () =>
                    {
                        await zoneViewModelRenderer.Stop();
                    });
                }
                return stopCommand;
            }
        }

        private ICommand nextCommand;
        public ICommand NextCommand
        {
            get
            {
                if (nextCommand == null)
                {
                    nextCommand = new DelegateCommand(async () =>
                    {
                        await zoneViewModelRenderer.Next();
                    });
                }
                return nextCommand;
            }
        }

        private ICommand setAVTransportUriCommand;
        public ICommand SetAVTransportUriCommand
        {
            get
            {
                if (setAVTransportUriCommand == null)
                {
                    setAVTransportUriCommand = new DelegateCommand<ElementBase>(async (param) =>
                    {
                    ElementBase element = await raumFeldService.BrowseMetaData(param.Id);
                        if (element != null && element.IsPlayable)
                        {
                            //Sent Notification to clear playlist of active renderer
                            //DoEmptyPlayList?.Invoke(this, null);

                            if (element.IsFolder)
                            {
                                ServiceActionReturnMessage messageContainer = await zoneViewModelRenderer.SetAVTransportUri(true, raumFeldService.GetMediaServerUDN(), element.Id, null, 0, element.BrowsedMetaData);
                                if (messageContainer.ActionStatus == ActionStatus.Error)
                                {
                                    //await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageContainer.ActionErrorCode, messageContainer.ActionMessage), UIService.GetResource("Error"));
                                    return;
                                }
                            }
                            else
                            {
                                ServiceActionReturnMessage messageItem = await zoneViewModelRenderer.SetAVTransportUri(false, raumFeldService.GetMediaServerUDN(), null, element.Id, -1, element.BrowsedMetaData);
                                if (messageItem.ActionStatus == ActionStatus.Error)
                                {
                                    //await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageItem.ActionErrorCode, messageItem.ActionMessage), UIService.GetResource("Error"));
                                    return;
                                }
                            }
                        }

                        //switch (element.Class)
                        //{
                        //    //Object_container_trackContainer,
                        //    //Object_container_trackContainer_napster,
                        //    //Object_item_audioItem_audioBroadcast_lastFM,
                        //    //Object_item_audioItem_audioBroadcast_rhapsody,
                        //    case "object.container":
                        //    case "object.container.albumContainer":
                        //    case "object.container.favoritesContainer":
                        //    case "object.container.genre.musicGenre":
                        //    case "object.container.person.musicArtist":
                        //    case "object.container.person.musicComposer":
                        //        break;
                        //    case "object.container.album.musicAlbum":
                        //    case "object.container.playlistContainer":
                        //    case "object.container.playlistContainer.shuffle":
                        //    case "object.container.storageFolder":
                        //    case "object.container.trackContainer.allTracks":
                        //    case "object.container.album.musicAlbum.compilation":
                        //        ServiceActionReturnMessage messageContainer = await ActiveRenderer.SetAVTransportUri(true, mediaServer.UDN, element.Id, null, 0, element.BrowsedMetaData);
                        //        if (messageContainer.ActionStatus == ActionStatus.Error)
                        //        {
                        //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageContainer.ActionErrorCode, messageContainer.ActionMessage), UIService.GetResource("Error"));
                        //        }
                        //        break;
                        //    case "object.item.audioItem.audioBroadcast.radio":
                        //        ServiceActionReturnMessage messageRadio = await ActiveRenderer.SetAVTransportUri(false, mediaServer.UDN, null, element.Id, -1, element.BrowsedMetaData);
                        //        if (messageRadio.ActionStatus == ActionStatus.Error)
                        //        {
                        //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageRadio.ActionErrorCode, messageRadio.ActionMessage), UIService.GetResource("Error"));
                        //        }
                        //        break;
                        //    case "object.item.audioItem.musicTrack":

                        //        Element parentelement = await browseMetaData(e.Value.ParentID);
                        //        if (parentelement != null)
                        //        {
                        //            //ServiceActionReturnMessage messageTrack = await ActiveRenderer.SetAVTransportUri(true, mediaServer.UDN, element.ParentID, element.Id, 0, parentelement.BrowsedMetaData);
                        //            ServiceActionReturnMessage messageTrack = await ActiveRenderer.SetAVTransportUri(false, mediaServer.UDN, null, element.Id, -1, element.BrowsedMetaData);
                        //            if (messageTrack.ActionStatus == ActionStatus.Error)
                        //            {
                        //                await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), messageTrack.ActionErrorCode, messageTrack.ActionMessage), UIService.GetResource("Error"));
                        //            }
                        //        }
                        //        break;
                        //    case "object.item.audioItem.audioBroadcast.lineIn":
                        //        //Play
                        //        break;
                        //    default:
                        //        break;
                        //}
                    });
                }
                return setAVTransportUriCommand;
            }
        }


        public ObservableCollection<IRoomViewModel> RoomViewModels { get; }

        public string Name { get; }
        public string Udn { get; }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                SetProperty(ref isActive, value, () =>
                {
                    IsActiveVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }
        private Visibility isActiveVisibility;
        public Visibility IsActiveVisibility
        {
            get { return isActiveVisibility; }
            set { SetProperty(ref isActiveVisibility, value); }
        }

        private double zoneVolume;
        public double ZoneVolume
        {
            get { return zoneVolume; }
            set { SetProperty(ref zoneVolume, value); }
        }

        private bool? zoneMute;
        public bool? ZoneMute
        {
            get { return zoneMute; }
            set { SetProperty(ref zoneMute, value); }
        }

        private int currentTrack;
        public int CurrentTrack
        {
            get { return currentTrack; }
            set { SetProperty(ref currentTrack, value); }
        }

        private int numberOfTracks;
        public int NumberOfTracks
        {
            get { return numberOfTracks; }
            set { SetProperty(ref numberOfTracks, value); }
        }

        public Uri CurrentTrackURI { get; set; }

        private TimeSpan currentTrackDuration;
        public TimeSpan CurrentTrackDuration
        {
            get { return currentTrackDuration; }
            set { SetProperty(ref currentTrackDuration, value); }
        }

        private ElementBase currentTrackMetaData;
        public ElementBase CurrentTrackMetaData
        {
            get { return currentTrackMetaData; }
            set { SetProperty(ref currentTrackMetaData, value); }
        }

        private ElementBase avTransportURIMetaData;
        public ElementBase AVTransportURIMetaData
        {
            get { return avTransportURIMetaData; }
            set { SetProperty(ref avTransportURIMetaData, value); }
        }

        private ObservableCollection<ElementBase> zoneViewModelTracks;
        public ObservableCollection<ElementBase> ZoneViewModelTracks
        {
            get { return zoneViewModelTracks; }
            set { SetProperty(ref zoneViewModelTracks, value); }
        }

        #region PlayModes

        private bool isEnabledImageCurrentItem;
        public bool IsEnabledImageCurrentItem
        {
            get { return isEnabledImageCurrentItem; }
            set { SetProperty(ref isEnabledImageCurrentItem, value, () => IsEnabledImageCurrentItemVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledImageCurrentItemVisibility;
        public Visibility IsEnabledImageCurrentItemVisibility
        {
            get { return isEnabledImageCurrentItemVisibility; }
            set { SetProperty(ref isEnabledImageCurrentItemVisibility, value); }
        }

        private bool isEnabledDescriptionCurrentItem;
        public bool IsEnabledDescriptionCurrentItem
        {
            get { return isEnabledDescriptionCurrentItem; }
            set { SetProperty(ref isEnabledDescriptionCurrentItem, value, () => IsEnabledDescriptionCurrentItemVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledDescriptionCurrentItemVisibility;
        public Visibility IsEnabledDescriptionCurrentItemVisibility
        {
            get { return isEnabledDescriptionCurrentItemVisibility; }
            set { SetProperty(ref isEnabledDescriptionCurrentItemVisibility, value); }
        }

        private bool isEnabledDurationCurrentItem;
        public bool IsEnabledDurationCurrentItem
        {
            get { return isEnabledDurationCurrentItem; }
            set { SetProperty(ref isEnabledDurationCurrentItem, value, () => IsEnabledDurationCurrentItemtVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledDurationCurrentItemtVisibility;
        public Visibility IsEnabledDurationCurrentItemtVisibility
        {
            get { return isEnabledDurationCurrentItemtVisibility; }
            set { SetProperty(ref isEnabledDurationCurrentItemtVisibility, value); }
        }

        private bool isEnabledPrevious;
        public bool IsEnabledPrevious
        {
            get { return isEnabledPrevious; }
            set { SetProperty(ref isEnabledPrevious, value, () => IsEnabledPreviousVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledPreviousVisibility;
        public Visibility IsEnabledPreviousVisibility
        {
            get { return isEnabledPreviousVisibility; }
            set
            {
                SetProperty(ref isEnabledPreviousVisibility, value);
            }
        }

        private bool isEnabledPlay;
        public bool IsEnabledPlay
        {
            get { return isEnabledPlay; }
            set { SetProperty(ref isEnabledPlay, value, () => IsEnabledPlayVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledPlayVisibility;
        public Visibility IsEnabledPlayVisibility
        {
            get { return isEnabledPlayVisibility; }
            set
            {
                SetProperty(ref isEnabledPlayVisibility, value);
            }
        }

        private bool isEnabledPause;
        public bool IsEnabledPause
        {
            get { return isEnabledPause; }
            set { SetProperty(ref isEnabledPause, value, () => IsEnabledPauseVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledPauseVisibility;
        public Visibility IsEnabledPauseVisibility
        {
            get { return isEnabledPauseVisibility; }
            set { SetProperty(ref isEnabledPauseVisibility, value); }
        }

        private bool isEnabledStop;
        public bool IsEnabledStop
        {
            get { return isEnabledStop; }
            set { SetProperty(ref isEnabledStop, value, () => IsEnabledStopVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledStopVisibility;
        public Visibility IsEnabledStopVisibility
        {
            get { return isEnabledStopVisibility; }
            set
            {
                SetProperty(ref isEnabledStopVisibility, value);
            }
        }

        private bool isEnabledNext;
        public bool IsEnabledNext
        {
            get { return isEnabledNext; }
            set { SetProperty(ref isEnabledNext, value, () => IsEnabledNextVisibility = value ? Visibility.Visible : Visibility.Collapsed); }
        }
        private Visibility isEnabledNextVisibility;
        public Visibility IsEnabledNextVisibility
        {
            get { return isEnabledNextVisibility; }
            set
            {
                SetProperty(ref isEnabledNextVisibility, value);
            }
        }

        private bool isNormal;
        public bool IsNormal
        {
            get { return isNormal; }
            set
            {
                SetProperty(ref isNormal, value, () => { if (value) { SetPlayModeCommand.Execute("NORMAL"); } });
            }
        }

        private bool isShuffle;
        public bool IsShuffle
        {
            get { return isShuffle; }
            set
            {
                SetProperty(ref isShuffle, value, () => { if (value) { SetPlayModeCommand.Execute("SHUFFLE"); } });
            }
        }

        private bool isRepeatOne;
        public bool IsRepeatOne
        {
            get { return isRepeatOne; }
            set
            {
                SetProperty(ref isRepeatOne, value, () => { if (value) { SetPlayModeCommand.Execute("REPEAT_ONE"); } });
            }
        }

        private bool isRepeatAll;
        public bool IsRepeatAll
        {
            get { return isRepeatAll; }
            set
            {
                SetProperty(ref isRepeatAll, value, () => { if (value) { SetPlayModeCommand.Execute("REPEAT_ALL"); } });
            }
        }

        private bool isRandom;
        public bool IsRandom
        {
            get { return isRandom; }
            set
            {
                SetProperty(ref isRandom, value, () => { if (value) { SetPlayModeCommand.Execute("RANDOM"); } });
            }
        }

        private bool isDirektOne;
        public bool IsDirektOne
        {
            get { return isDirektOne; }
            set
            {
                SetProperty(ref isDirektOne, value, () => { if (value) { SetPlayModeCommand.Execute("DIREKT_1"); } });
            }
        }

        private bool isIntro;
        public bool IsIntro
        {
            get { return isIntro; }
            set
            {
                SetProperty(ref isIntro, value, () => { if (value) { SetPlayModeCommand.Execute("INTRO"); } });
            }
        }

        #endregion

        public ZoneViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, ICachingService cachingServiceInstance, IShellViewModel shellViewModelInstance, IRaumFeldService raumFeldServiceInstance, MediaRenderer zoneViewModelRendererInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            cachingService = cachingServiceInstance;
            shellViewModel = shellViewModelInstance;
            raumFeldService = raumFeldServiceInstance;
            zoneViewModelRenderer = zoneViewModelRendererInstance;

            RoomViewModels = new ObservableCollection<IRoomViewModel>();
            ZoneViewModelTracks = new ObservableCollection<ElementBase>();

            Name = zoneViewModelRendererInstance.Name != string.Empty ? zoneViewModelRendererInstance.Name : "UnassignedRooms".GetLocalized();
            Udn = zoneViewModelRendererInstance.Udn != string.Empty ? zoneViewModelRendererInstance.Name : "UnassignedRooms".GetLocalized();

            eventAggregator.GetEvent<MuteChangedEvent>().Subscribe(onMuteChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<VolumeChangedEvent>().Subscribe(onVolumeChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<RoomMutesChangedEvent>().Subscribe(onRoomMutesChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<RoomVolumesChangedEvent>().Subscribe(onRoomVolumesChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<AVTransportURIChangedEvent>().Subscribe(onAVTransportURIChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<AVTransportURIMetaDataChangedEvent>().Subscribe(onAVTransportURIMetaDataChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<CurrentTrackChangedEvent>().Subscribe(onCurrentTrackChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<CurrentTrackURIChangedEvent>().Subscribe(onCurrentTrackURIChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<CurrentPlayModeChangedEvent>().Subscribe(onCurrentPlayModeChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<CurrentTrackMetaDataChangedEvent>().Subscribe(onCurrentTrackMetaDataChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<CurrentTransportActionsChangedEvent>().Subscribe(onCurrentTransportActionsChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<NumberOfTracksChangedEvent>().Subscribe(onNumberOfTracksChanged,
                         ThreadOption.UIThread, false,
                         device => device.MediaDevice == zoneViewModelRenderer);
            eventAggregator.GetEvent<RoomStatesChangedEvent>().Subscribe(onRoomStatesChanged,
                        ThreadOption.UIThread, false,
                        device => device.MediaDevice == zoneViewModelRenderer);

            GetZoneVolumeCommand.Execute(null);
            GetZoneMuteCommand.Execute(null);
            GetCurrentTransportActionsCommand.Execute(null);
            GetTransportSettingsCommand.Execute(null);
            GetMediaInfoCommand.Execute(null);
            GetPositionInfoCommand.Execute(null);
        }

        /// <summary>
        /// Get room volume
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetRoomVolume(string roomUdn)
        {
            if (zoneViewModelRenderer == null) { return 0; }
            ServiceActionReturnMessage message = await zoneViewModelRenderer.GetRoomVolume(roomUdn);
            if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is double volume)
            {
                return volume;
            }
            else { return 0; }
        }

        /// <summary>
        /// Set room mute state
        /// </summary>
        /// <param name="muteStatus"></param>
        /// <returns></returns>
        public async Task<bool> GetRoomMute(string roomUdn)
        {
            if (zoneViewModelRenderer == null) { return false; }
            ServiceActionReturnMessage message = await zoneViewModelRenderer.GetRoomMute(roomUdn);
            if (message.ActionStatus == ActionStatus.Okay && message.ReturnValue is string mute)
            {
                return mute == "1";
            }
            else { return false; }
        }

        /// <summary>
        /// Set room volume
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public async Task SetRoomVolume(string roomUdn, double volume)
        {
            if (zoneViewModelRenderer == null) { return; }
            ServiceActionReturnMessage message = await zoneViewModelRenderer.SetRoomVolume(roomUdn, volume.ToString());
        }

        /// <summary>
        /// Set room mute state
        /// </summary>
        /// <param name="muteStatus"></param>
        /// <returns></returns>
        public async Task SetRoomMute(string roomUdn, bool muteSate)
        {
            if (zoneViewModelRenderer == null) { return ; }
            ServiceActionReturnMessage message = await zoneViewModelRenderer.SetRoomMute(roomUdn, muteSate ? "1" : "0");
        }

        /// <summary>
        /// Set room state
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public async Task SetRommState(string roomUdn, bool isActive)
        {
            if (zoneViewModelRenderer == null) { return ; }
            ServiceActionReturnMessage message;

            if (isActive)
            {
                message = await zoneViewModelRenderer.LeaveStandby(roomUdn);
            }
            else
            {
                message = await zoneViewModelRenderer.EnterManualStandby(roomUdn);
            }
        }

        private void onMuteChanged(RaumFeldEvent args)
        {
            if (args.ChangedValues.TryGetValue("val", out string mute))
            {
                ZoneMute = (mute == "1");
            }
        }
        private void onVolumeChanged(RaumFeldEvent args)
        {
            if (args.ChangedValues.TryGetValue("val", out string volume))
            {
                ZoneVolume = (double.Parse(volume));
            }
        }
        private void onRoomMutesChanged(RaumFeldEvent args)
        {
            //val = "uuid:29e07ad9-224f-4160-a2bc-61d17845182a=0" />
            string[] stringSeparators = new string[] { "=", "," };
            string[] splitValue = new string[0];

            if (args.ChangedValues.TryGetValue("val", out string mute))
            {
                // Split delimited by another string and return all non-empty elements.
                splitValue = mute.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                if ((RoomViewModels?.Count() ?? 0) == 0) { return; }
                IRoomViewModel room = RoomViewModels.Select(r => r).Where(r => r.Udn == splitValue[0]).FirstOrDefault();

                if (room != null)
                {
                    room.RoomMute = (splitValue[1] == "1");
                }
            }
        }
        private void onRoomVolumesChanged(RaumFeldEvent args)
        {
            //val = "uuid:29e07ad9-224f-4160-a2bc-61d17845182a=100" />
            string[] stringSeparators = new string[] { "=", "," };
            string[] splitValue = new string[0];

            if (args.ChangedValues.TryGetValue("val", out string volume))
            {
                // Split delimited by another string and return all non-empty elements.
                splitValue = volume.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                if ((RoomViewModels?.Count() ?? 0) == 0) { return; }
                IRoomViewModel room = RoomViewModels.Select(r => r).Where(r => r.Udn == splitValue[0]).FirstOrDefault();

                if (room != null) { room.RoomVolume = double.Parse(splitValue[1]); }
            }
        }
        private void onAVTransportURIChanged(RaumFeldEvent args)
        {
            // val = "dlna-playcontainer://uuid%3A1a636014-b38f-420e-8456-3792ba3279e4?sid=urn%3Aupnp-org%3AserviceId%3AContentDirectory&amp;cid=0%2FMy Music%2FAlbums%2FThe%2520Notwist+12&amp;md=0&amp;fii=0" />
            //if (e.Value.ChangedValues.TryGetValue("val", out string avtransporturi))
            //{
            //    // Get Content?!
            //}
        }
        private async void onAVTransportURIMetaDataChanged(RaumFeldEvent args)
        {
            // val = "&lt;DIDL-Lite xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot; xmlns:raumfeld=&quot;urn:schemas-raumfeld-com:meta-data/raumfeld&quot; xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xmlns:dlna=&quot;urn:schemas-dlna-org:metadata-1-0/&quot; lang=&quot;en&quot;&gt;&lt;container parentID=&quot;0/My Music/Albums&quot; id=&quot;0/My Music/Albums/The%20Notwist+12&quot; restricted=&quot;1&quot; childCount=&quot;9&quot;&gt;&lt;raumfeld:name&gt;Album&lt;/raumfeld:name&gt;&lt;upnp:class&gt;object.container.album.musicAlbum&lt;/upnp:class&gt;&lt;raumfeld:section&gt;My Music&lt;/raumfeld:section&gt;&lt;upnp:artist&gt;The Notwist&lt;/upnp:artist&gt;&lt;dc:date&gt;1995&lt;/dc:date&gt;&lt;upnp:album&gt;12&lt;/upnp:album&gt;&lt;upnp:albumArtURI dlna:profileID=&quot;JPEG_TN&quot;&gt;http://192.168.0.18:47366/?albumArtist=The%20Notwist&amp;amp;album=12&lt;/upnp:albumArtURI&gt;&lt;raumfeld:totalPlaytime&gt;0:39:07&lt;/raumfeld:totalPlaytime&gt;&lt;dc:title&gt;12&lt;/dc:title&gt;&lt;/container&gt;&lt;/DIDL-Lite&gt;" />
            if (args.ChangedValues.TryGetValue("val", out string avtransporturimetadata))
            {
                DIDLLite didl = avtransporturimetadata.Deserialize<DIDLLite>();
                if ((didl?.Containers?.Count() ?? 0) != 0)
                {
                    if (await raumFeldService.BrowseChildren(ZoneViewModelTracks, didl.Containers?.First().Id, true))
                    {
                        setSelection(CurrentTrackMetaData);
                    }
                    else { ZoneViewModelTracks.Clear(); }
                }
            }
        }
        private void onCurrentTrackChanged(RaumFeldEvent args)
        {
            // val = "7" />
            if (args.ChangedValues.TryGetValue("val", out string currenttrack) && currenttrack != "0")
            {
                CurrentTrack = Int32.Parse(currenttrack);
            }
        }
        private void onCurrentTrackURIChanged(RaumFeldEvent args)
        {
            // val = "http://192.168.0.18:37665/redirect?uri=smb%3A%2F%2Fjc-station%2Fmusic%2F%2FMP3%2FThe%2520Notwist%2F12%2F07%2520The%2520String.mp3" />
            //if (args.ChangedValuea.TryGetValue("val", out string currenttrackuri))
            //{
            //    Get Content?!
            //}
        }
        private void onCurrentPlayModeChanged(RaumFeldEvent args)
        {
            // val = "REPEAT_ALL" />
            if (args.ChangedValues.TryGetValue("val", out string currentplaymode))
            {
                resetPlayMode();
                switch (currentplaymode.ToUpper())
                {
                    case "NORMAL":
                        IsNormal = true;
                        break;
                    case "SHUFFLE":
                        IsShuffle = true;
                        break;
                    case "REPEAT_ONE":
                        IsRepeatOne = true;
                        break;
                    case "REPEAT_ALL":
                        IsRepeatAll = true;
                        break;
                    case "RANDOM":
                        IsRandom = true;
                        break;
                    case "DIRECT_1":
                        IsDirektOne = true;
                        break;
                    case "INTRO":
                        IsIntro = true;
                        break;
                    default:
                        break;
                }
            }
        }
        private void onCurrentTrackMetaDataChanged(RaumFeldEvent args)
        {
            // val = "&lt;DIDL-Lite xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot; xmlns:raumfeld=&quot;urn:schemas-raumfeld-com:meta-data/raumfeld&quot; xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xmlns:dlna=&quot;urn:schemas-dlna-org:metadata-1-0/&quot; lang=&quot;en&quot;&gt;&lt;item parentID=&quot;0/My Music/Albums/The%20Notwist+12&quot; id=&quot;0/My Music/Albums/The%20Notwist+12/75725c9134fd69b824706f8a3b3030c4&quot; restricted=&quot;1&quot;&gt;&lt;raumfeld:name&gt;Track&lt;/raumfeld:name&gt;&lt;upnp:class&gt;object.item.audioItem.musicTrack&lt;/upnp:class&gt;&lt;raumfeld:section&gt;My Music&lt;/raumfeld:section&gt;&lt;dc:title&gt;The String&lt;/dc:title&gt;&lt;upnp:album&gt;12&lt;/upnp:album&gt;&lt;upnp:artist&gt;The Notwist&lt;/upnp:artist&gt;&lt;upnp:genre&gt;Punk Rock&lt;/upnp:genre&gt;&lt;dc:creator&gt;The Notwist&lt;/dc:creator&gt;&lt;upnp:originalTrackNumber&gt;7&lt;/upnp:originalTrackNumber&gt;&lt;dc:date&gt;1995-01-01&lt;/dc:date&gt;&lt;upnp:albumArtURI dlna:profileID=&quot;JPEG_TN&quot;&gt;http://192.168.0.18:47366/?artist=The%20Notwist&amp;amp;albumArtist=The%20Notwist&amp;amp;album=12&amp;amp;track=The%20String&lt;/upnp:albumArtURI&gt;&lt;res protocolInfo=&quot;http-get:*:audio/mpeg:DLNA.ORG_PN=MP3&quot; size=&quot;5636735&quot; duration=&quot;0:04:31.000&quot; bitrate=&quot;163840&quot; sampleFrequency=&quot;44100&quot; nrAudioChannels=&quot;2&quot; sourceName=&quot;music on jc-station&quot; sourceType=&quot;smb&quot; sourceID=&quot;uuid:39cb689d-3804-43da-a14e-21102a1f50ec&quot;&gt;http://192.168.0.18:37665/redirect?uri=smb%3A%2F%2Fjc-station%2Fmusic%2F%2FMP3%2FThe%2520Notwist%2F12%2F07%2520The%2520String.mp3&lt;/res&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;" />
            if (args.ChangedValues.TryGetValue("val", out string currenttrackmetadata))
            {
                DIDLLite didl = currenttrackmetadata.Deserialize<DIDLLite>();
                if ((didl?.Items?.Count() ?? 0) != 0)
                {
                    CurrentTrackMetaData = PrismUnityApplication.Current.Container.Resolve<ElementBase>("DIDLItem",
                                        new DependencyOverride(typeof(IEventAggregator), eventAggregator),
                                        new DependencyOverride(typeof(ICachingService), cachingService),
                                        new DependencyOverride(typeof(DIDLItem), didl?.Items?.FirstOrDefault()));

                    setSelection(CurrentTrackMetaData);
                }
            }
        }
        private void onCurrentTransportActionsChanged(RaumFeldEvent args)
        {
            // val = "Next,Pause,Previous,Repeat,Seek,Shuffle,Stop" />

            string[] stringSeparators = new string[] { "=", "," };
            string[] splitValue = new string[0];

            if (args.ChangedValues.TryGetValue("val", out string currenttransportactions))
            {
                if (!string.IsNullOrEmpty(currenttransportactions))
                {
                    // Split delimited by another string and return all non-empty elements.
                    splitValue = currenttransportactions.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    resetPlayState();
                    setPlayState(splitValue);
                }
            }
        }
        private void onNumberOfTracksChanged(RaumFeldEvent args)
        {
            // val = "9" />
            if (args.ChangedValues.TryGetValue("val", out string numberoftracks))
            {
                NumberOfTracks = Int32.Parse(numberoftracks);
            }
        }
        private void onRoomStatesChanged(RaumFeldEvent args)
        {
            // val = "uuid:29e07ad9-224f-4160-a2bc-61d17845182a=PLAYING" />
            string[] stringSeparators = new string[] { "=", "," };
            string[] splitValue = new string[0];

            if (args.ChangedValues.TryGetValue("val", out string roomstates))
            {
                splitValue = roomstates.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                if ((RoomViewModels?.Count() ?? 0) == 0 || (splitValue?.Count() ?? 0) == 0) { return; }
                IRoomViewModel room = RoomViewModels.Select(r => r).Where(r => r.Udn == splitValue[0]).FirstOrDefault();

                if (room != null)
                {
                    switch (splitValue[1])
                    {
                        case "PLAYING":
                            break;
                        case "STOPPED":
                            break;
                        case "TRANSITIONING":
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void InitVolumes()
        {
            //await GetZoneVolume();
            //await GetZoneMute();
            //await GetRoomVolume();
            //await GetRoomMute();
        }

        #region Private Methods

        /// <summary>
        /// Set IsSelected flag for item
        /// </summary>
        /// <param name="current">Current item</param>
        private void setSelection(ElementBase current)
        {
            if ((ZoneViewModelTracks?.Count() ?? 0) > 0 && current != null)
            {
                foreach (ElementBase element in ZoneViewModelTracks)
                {
                    if (element.Id == current.Id) { element.IsSelected = true; }
                    else { element.IsSelected = false; }
                }
            }
        }

        /// <summary>
        /// Set all IsEnabled flags to false
        /// </summary>
        private void resetPlayState()
        {
            IsEnabledPrevious = false;
            IsEnabledPlay = false;
            IsEnabledPause = false;
            IsEnabledStop = false;
            IsEnabledNext = false;
        }

        /// <summary>
        /// Set IsEnabled flag
        /// </summary>
        /// <param name="transportActions"></param>
        private void setPlayState(string[] transportActions)
        {
            foreach (var action in transportActions)
            {
                switch (action.ToUpper())
                {
                    case "PREVIOUS":
                        IsEnabledPrevious = true;
                        break;
                    case "PLAY":
                        IsEnabledPlay = true;
                        break;
                    case "PAUSE":
                        IsEnabledPause = true;
                        break;
                    case "STOP":
                        IsEnabledStop = true;
                        break;
                    case "NEXT":
                        IsEnabledNext = true;
                        break;
                    default:
                        break;
                }
            }

            //shellViewModel.RefreshBinding("ActiveZoneViewModel");
        }

        /// <summary>
        /// Set all Playmodes flags to false
        /// </summary>
        private void resetPlayMode()
        {
            IsNormal = false;
            IsShuffle = false;
            IsRepeatOne = false;
            IsRepeatAll = false;
            IsRandom = false;
            IsDirektOne = false;
            IsIntro = false;
        }

        /// <summary>
        /// Set all Playmodes flags to false
        /// </summary>
        private void setPlayMode(string playmode)
        {
            switch (playmode.ToUpper())
            {
                case "NORMAL":
                    IsNormal = true;
                    break;
                case "SHUFFLE":
                    IsShuffle = true;
                    break;
                case "REPEAT_ONE":
                    IsRepeatOne = true;
                    break;
                case "REPEAT_ALL":
                    IsRepeatAll = true;
                    break;
                case "RANDOM":
                    IsRandom = true;
                    break;
                case "DIREKT_1":
                    IsDirektOne = true;
                    break;
                case "INTRO":
                    IsIntro = true;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region UIEvents

        /// <summary>
        /// Event when Zone ListviewItem is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //public async void ZoneListView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    if (e.ClickedItem is Element element)
        //    {
        //        ServiceActionReturnMessage message = await MediaRenderer.Seek("TRACK_NR", element.Index.ToString());
        //        if (message.ActionStatus == ActionStatus.Error)
        //        {
        //            await UIService.ShowDialogAsync(string.Format("{0} {1}: {2}", UIService.GetResource("Error"), message.ActionErrorCode, message.ActionMessage), "ZoneListView_ItemClick");
        //        }
        //    }
        //}

        #endregion
    }
}
