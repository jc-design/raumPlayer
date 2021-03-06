﻿using raumPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace raumPlayer.Interfaces
{
    public interface IElementBase
    {
        bool IsFolder { get; }
        //string ParentID { get; }
        //string Id { get; }
        //string RefId { get; }
        //int Restricted { get; }
        //int ChildCount { get; }
        //int NumberOfAlbums { get; }
        //string Name { get; }
        //string Class { get; }
        //string Section { get; }
        //string TotalPlayTime { get; }
        //string Title { get; }
        //string Date { get; }
        //string Album { get; }
        //int OriginalTrackNumber { get; }
        //string Artist { get; }
        //string Genre { get; }
        //string Creator { get; }
        //string Description { get; }
        //string AlbumArtProfileId { get; }
        //string AlbumArtUri { get; }
        //string Resource { get; }
        //string ResourceProtocolInfo { get; }
        //string ResourceResolution { get; }
        //string ResourceSize { get; }
        //string Duration { get; }

        //bool IsPlayable { get; set; }
        //bool IsSelected { get; set; }
        //Visibility IsSelectedVisibility { get; set; }
        //int Index { get; set; }

        //string VisibleCount { get; }
        //Visibility VisibleCountVisibility { get; }

        //string Line1 { get; }
        //string Line2 { get; }

        //BitmapImage ImageArt { get; set; }
        //SolidColorBrush AverageColorBrushImageArt { get; set; }

        //string BrowsedMetaData { get; set; }

        ICommand IntializeCommand { get; }
    }
}
