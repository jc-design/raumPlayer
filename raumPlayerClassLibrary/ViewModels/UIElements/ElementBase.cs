using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Upnp;
using Windows.UI.Xaml;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using System.Windows.Input;
using Prism.Commands;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI;
using raumPlayer.Services;

namespace raumPlayer.ViewModels
{
    public class ElementBase : ViewModelBase, IElementBase
    {
        public bool IsFolder { get; }
        public string ParentID { get; }
        public string Id { get; }
        public string RefId { get; }
        public int Restricted { get; }
        public int ChildCount { get; }
        public int NumberOfAlbums { get; }
        public string Name { get; }
        public string Class { get; }
        public string Section { get; }
        public string TotalPlayTime { get; }
        public string Title { get; }
        public string Date { get; }
        public string Album { get; }
        public int OriginalTrackNumber { get; }
        public string Artist { get; }
        public string Genre { get; }
        public string Creator { get; }
        public string Description { get; }
        public string AlbumArtProfileId { get; }
        public string AlbumArtUri { get; }
        public string Resource { get; }
        public string ResourceProtocolInfo { get; }
        public string ResourceResolution { get; }
        public string ResourceSize { get; }
        public string Duration { get; }

        private bool isPlayable;
        public bool IsPlayable
        {
            get { return isPlayable; }
            set { SetProperty(ref isPlayable, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        private Visibility isSelectedVisibility;
        public Visibility IsSelectedVisibility
        {
            get { return isSelectedVisibility; }
            set { SetProperty(ref isSelectedVisibility, value); }
        }

        private int index;
        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        public string VisibleCount { get; }
        public Visibility VisibleCountVisibility { get; }

        public string Line1 { get; }
        public string Line2 { get; }

        public ElementBase Element { get; }
        public string BrowsedMetaData { get; set; }

        private BitmapImage imageArt;
        public BitmapImage ImageArt
        {
            get { return imageArt; }
            set { SetProperty(ref imageArt, value); }
        }
        private SolidColorBrush averageColorImageArt;
        public SolidColorBrush AverageColorImageArt
        {
            get { return averageColorImageArt; }
            set { SetProperty(ref averageColorImageArt, value); }
        }

        public ElementBase() { }

        public ElementBase(DIDLItem item)
        {
            IsFolder = false;
            ParentID = item?.ParentID ?? string.Empty;
            Id = item?.Id ?? string.Empty;
            RefId = item?.RefId ?? string.Empty;
            Restricted = item.Restricted;
            ChildCount = 0;
            NumberOfAlbums = 0;
            Name = item?.Name ?? string.Empty;
            Class = item?.Class ?? string.Empty;
            Section = item?.Section ?? string.Empty;
            TotalPlayTime = string.Empty;
            Title = item?.Title ?? string.Empty;
            Date = string.Empty;
            Album = item?.Album ?? string.Empty;
            OriginalTrackNumber = item.OriginalTrackNumber;
            Artist = item?.Artist ?? string.Empty;
            Genre = item?.Genre ?? string.Empty;
            Creator = item?.Creator ?? string.Empty;
            Description = string.Empty;
            AlbumArtProfileId = item?.AlbumArtUri?.ProfileId ?? string.Empty;
            AlbumArtUri = item?.AlbumArtUri?.AlbumArtUri ?? "ms-appx:///Assets/disc_gray.png";
            Resource = item?.Res?.Value ?? string.Empty;
            ResourceProtocolInfo = item?.Res?.ProtocolInfo ?? string.Empty;
            ResourceResolution = item?.Res?.Resolution ?? string.Empty;
            ResourceSize = item?.Res?.Size ?? string.Empty;
            if (TimeSpan.TryParse(item?.Res?.Duration ?? string.Empty, out TimeSpan timeSpan))
            {
                Duration = timeSpan.ToString(@"mm\:ss");
            }
            else
            {
                Duration = item?.Res?.Duration ?? string.Empty;
            }

            IsPlayable = true;
            IsSelected = false;
            IsSelectedVisibility = Visibility.Collapsed;
            Index = 0;

            VisibleCount = item.OriginalTrackNumber > 0 ? item.OriginalTrackNumber.ToString() : string.Empty;
            VisibleCountVisibility = item.OriginalTrackNumber > 0 ? Visibility.Visible : Visibility.Collapsed;

            Line1 = item?.Album ?? string.Empty;
            if (string.IsNullOrEmpty(item.Artist) || string.IsNullOrEmpty(item.Genre))
            {
                Line2 = string.Format("{0}{1}", item?.Artist ?? string.Empty, item?.Genre ?? string.Empty);
            }
            else
            {
                Line2 = string.Format("{0} - {1}", item?.Artist ?? string.Empty, item?.Genre ?? string.Empty);
            }

            Element = this;
            BrowsedMetaData = string.Empty;
        }
        public ElementBase(DIDLContainer container)
        {
            IsFolder = true;
            ParentID = container?.ParentID ?? string.Empty;
            Id = container?.Id ?? string.Empty;
            RefId = container?.RefId ?? string.Empty;
            Restricted = container.Restricted;
            ChildCount = container.ChildCount;
            NumberOfAlbums = container.NumberOfAlbums;
            Name = container?.Name ?? string.Empty;
            Class = container?.Class ?? string.Empty;
            Section = container?.Section ?? string.Empty;
            TotalPlayTime = container?.TotalPlayTime ?? string.Empty;
            Title = container?.Title ?? string.Empty;
            Date = container?.Date ?? string.Empty;
            Album = container?.Album ?? string.Empty;
            OriginalTrackNumber = 0;
            Artist = container?.Artist ?? string.Empty;
            Genre = container?.Genre ?? string.Empty;
            Creator = container?.Creator ?? string.Empty;
            Description = container?.Description ?? string.Empty;
            AlbumArtProfileId = container?.AlbumArtUri?.ProfileId ?? string.Empty;
            AlbumArtUri = container?.AlbumArtUri?.AlbumArtUri ?? "ms-appx:///Assets/disc_gray.png";
            Resource = container?.Res?.Value ?? string.Empty;
            ResourceProtocolInfo = container?.Res?.ProtocolInfo ?? string.Empty;
            ResourceResolution = container?.Res?.Resolution ?? string.Empty;
            ResourceSize = container?.Res?.Size ?? string.Empty;
            if (TimeSpan.TryParse(container?.Res?.Duration ?? string.Empty, out TimeSpan timeSpan))
            {
                Duration = timeSpan.ToString(@"mm\:ss");
            }
            else
            {
                Duration = container?.Res?.Duration ?? string.Empty;
            }

            switch (container.Class)
            {
                case "object.container":
                case "object.container.albumContainer":
                case "object.container.favoritesContainer":
                case "object.container.genre.musicGenre":
                case "object.container.person.musicArtist":
                case "object.container.person.musicComposer":
                    IsPlayable = false;
                    break;
                case "object.container.album.musicAlbum":
                case "object.container.playlistContainer":
                case "object.container.playlistContainer.shuffle":
                case "object.container.storageFolder":
                case "object.container.trackContainer.allTracks":
                case "object.container.album.musicAlbum.compilation":
                    IsPlayable = true;
                    break;
                default:
                    IsPlayable = false;
                    break;
            }

            IsSelected = false;
            IsSelectedVisibility = Visibility.Collapsed;
            Index = 0;

            VisibleCount = container.ChildCount > 0 ? container.ChildCount.ToString() : string.Empty;
            //VisibleCountVisibility = container.ChildCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            VisibleCountVisibility = Visibility.Collapsed;

            Line1 = container?.Album ?? string.Empty;
            if (string.IsNullOrEmpty(container.Artist) || string.IsNullOrEmpty(container.Genre))
            {
                Line2 = string.Format("{0}{1}", container?.Artist ?? string.Empty, container?.Genre ?? string.Empty);
            }
            else
            {
                Line2 = string.Format("{0} - {1}", container?.Artist ?? string.Empty, container?.Genre ?? string.Empty);
            }

            Element = this;
            BrowsedMetaData = string.Empty;
        }
    }
}
