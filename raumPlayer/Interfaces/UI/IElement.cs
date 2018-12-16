using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace raumPlayer.Interfaces
{
    public interface IElement
    {
        bool IsFolder { get; }
        string ParentID { get; }
        string Id { get; }
        string RefId { get; }
        int Restricted { get; }
        int ChildCount { get; }
        int NumberOfAlbums { get; }
        string Name { get; }
        string Class { get; }
        string Section { get; }
        string TotalPlayTime { get; }
        string Title { get; }
        string Date { get; }
        string Album { get; }
        int OriginalTrackNumber { get; }
        string Artist { get; }
        string Genre { get; }
        string Creator { get; }
        string Description { get; }
        string AlbumArtProfileId { get; }
        string AlbumArtUri { get; }
        string Resource { get; }
        string ResourceProtocolInfo { get; }
        string ResourceResolution { get; }
        string ResourceSize { get; }
        string Duration { get; }

        bool IsPlayable { get; set; }
        bool IsSelected { get; set; }
        Visibility IsSelectedVisibility { get; set; }
        int Index { get; set; }

        string VisibleCount { get; }
        Visibility VisibleCountVisibility { get; }

        string Line1 { get; }
        string Line2 { get; }

        IElement Element { get; }

        string BrowsedMetaData { get; set; }
    }
}
