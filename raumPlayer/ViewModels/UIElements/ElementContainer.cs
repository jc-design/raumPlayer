using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Upnp;
using Windows.UI.Xaml;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;

namespace raumPlayer.ViewModels
{
    public class ElementContainer : ViewModelBase, IElement
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

        public IElement Element { get; }
        public string BrowsedMetaData { get; set; }

        public ElementContainer(DIDLContainer didl)
        {
            IsFolder = true;
            ParentID = didl?.ParentID ?? string.Empty;
            Id = didl?.Id ?? string.Empty;
            RefId = didl?.RefId ?? string.Empty;
            Restricted = didl.Restricted;
            ChildCount = didl.ChildCount;
            NumberOfAlbums = didl.NumberOfAlbums;
            Name = didl?.Name ?? string.Empty;
            Class = didl?.Class ?? string.Empty;
            Section = didl?.Section ?? string.Empty;
            TotalPlayTime = didl?.TotalPlayTime ?? string.Empty;
            Title = didl?.Title ?? string.Empty;
            Date = didl?.Date ?? string.Empty;
            Album = didl?.Album ?? string.Empty;
            OriginalTrackNumber = 0;
            Artist = didl?.Artist ?? string.Empty;
            Genre = didl?.Genre ?? string.Empty;
            Creator = didl?.Creator ?? string.Empty;
            Description = didl?.Description ?? string.Empty;
            AlbumArtProfileId = didl?.AlbumArtUri?.ProfileId ?? string.Empty;
            AlbumArtUri = didl?.AlbumArtUri?.AlbumArtUri ?? "ms-appx:///Assets/disc.png";
            Resource = didl?.Res?.Value ?? string.Empty;
            ResourceProtocolInfo = didl?.Res?.ProtocolInfo ?? string.Empty;
            ResourceResolution = didl?.Res?.Resolution ?? string.Empty;
            ResourceSize = didl?.Res?.Size ?? string.Empty;
            if (TimeSpan.TryParse(didl?.Res?.Duration ?? string.Empty, out TimeSpan timeSpan))
            {
                Duration = timeSpan.ToString(@"mm\:ss");
            }
            else
            {
                Duration = didl?.Res?.Duration ?? string.Empty;
            }

            switch (didl.Class)
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

            VisibleCount = didl.ChildCount > 0 ? didl.ChildCount.ToString() : string.Empty;
            VisibleCountVisibility = didl.ChildCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            Line1 = didl?.Album ?? string.Empty;
            if (string.IsNullOrEmpty(didl.Artist) || string.IsNullOrEmpty(didl.Genre))
            {
                Line2 = string.Format("{0}{1}", didl?.Artist ?? string.Empty, didl?.Genre ?? string.Empty);
            }
            else
            {
                Line2 = string.Format("{0} - {1}", didl?.Artist ?? string.Empty, didl?.Genre ?? string.Empty);
            }

            Element = this;
            BrowsedMetaData = string.Empty;
        }
    }
}
