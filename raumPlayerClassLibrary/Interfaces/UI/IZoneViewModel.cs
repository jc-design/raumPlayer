using raumPlayer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace raumPlayer.Interfaces
{
    public interface IZoneViewModel
    {
        string Name { get; }
        string Udn { get; }
        ObservableCollection<IRoomViewModel> RoomViewModels { get; }

        bool IsActive { get; set; }
        Visibility IsActiveVisibility { get; set; }
        double ZoneVolume { get; set; }
        bool? ZoneMute { get; set; }
        int CurrentTrack { get; set; }
        int NumberOfTracks { get; set; }
        Uri CurrentTrackURI { get; set; }
        TimeSpan CurrentTrackDuration { get; set; }
        ElementItem CurrentTrackMetaData { get; set; }
        //string AlbumArtUri { get; set; }
        ElementBase AVTransportURIMetaData { get; set; }
        ObservableCollection<ElementBase> ZoneViewModelTracks { get; set; }

        bool IsEnabledImageCurrentItem { get; set; }
        Visibility IsEnabledImageCurrentItemVisibility { get; set; }
        bool IsEnabledDescriptionCurrentItem { get; set; }
        Visibility IsEnabledDescriptionCurrentItemVisibility { get; set; }
        bool IsEnabledDurationCurrentItem { get; set; }
        Visibility IsEnabledDurationCurrentItemtVisibility { get; set; }
        bool IsEnabledPrevious { get; set; }
        Visibility IsEnabledPreviousVisibility { get; set; }
        bool IsEnabledPlay { get; set; }
        Visibility IsEnabledPlayVisibility { get; set; }
        bool IsEnabledPause { get; set; }
        Visibility IsEnabledPauseVisibility { get; set; }
        bool IsEnabledStop { get; set; }
        Visibility IsEnabledStopVisibility { get; set; }
        bool IsEnabledNext { get; set; }
        Visibility IsEnabledNextVisibility { get; set; }

        bool IsNormal { get; set; }
        bool IsShuffle { get; set; }
        bool IsRepeatOne { get; set; }
        bool IsRepeatAll { get; set; }
        bool IsRandom { get; set; }
        bool IsDirektOne { get; set; }
        bool IsIntro { get; set; }

        ICommand GetZoneVolumeCommand { get; }
        ICommand GetZoneMuteCommand { get; }
        ICommand GetCurrentTransportActionsCommand { get; }
        ICommand GetTransportSettingsCommand { get; }
        ICommand GetMediaInfoCommand { get; }
        ICommand GetPositionInfoCommand { get; }

        ICommand SetZoneVolumeCommand { get; }
        ICommand SetZoneMuteCommand { get; }

        Task<double> GetRoomVolume(string roomUdn);
        Task<bool> GetRoomMute(string roomUdn);

        Task SetRommState(string roomUdn, bool IsActive);
        Task SetRoomVolume(string roomUdn, double roomVolume);
        Task SetRoomMute(string roomUdn, bool muteSate);

        ICommand PreviousCommand { get; }
        ICommand PlayCommand { get; }
        ICommand PauseCommand { get; }
        ICommand StopCommand { get; }
        ICommand NextCommand { get; }
    }
}
