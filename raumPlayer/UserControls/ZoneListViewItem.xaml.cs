using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class ZoneListViewItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ZoneListViewItem), new PropertyMetadata(string.Empty));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty = DependencyProperty.Register(nameof(Album), typeof(string), typeof(ZoneListViewItem), new PropertyMetadata(string.Empty));
        public string Album
        {
            get { return (string)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(nameof(Artist), typeof(string), typeof(ZoneListViewItem), new PropertyMetadata(string.Empty));
        public string Artist
        {
            get { return (string)GetValue(ArtistProperty); }
            set { SetValue(ArtistProperty, value); }
        }

        public static readonly DependencyProperty GenreProperty = DependencyProperty.Register(nameof(Genre), typeof(string), typeof(ZoneListViewItem), new PropertyMetadata(string.Empty));
        public string Genre
        {
            get { return (string)GetValue(GenreProperty); }
            set { SetValue(GenreProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(string), typeof(ZoneListViewItem), new PropertyMetadata(string.Empty));
        public string Duration
        {
            get { return (string)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedVisibilityProperty = DependencyProperty.Register(nameof(IsSelectedVisibility), typeof(Visibility), typeof(ZoneListViewItem), new PropertyMetadata(Visibility.Collapsed));
        public Visibility IsSelectedVisibility
        {
            get { return (Visibility)GetValue(IsSelectedVisibilityProperty); }
            set { SetValue(IsSelectedVisibilityProperty, value); }
        }

        public ZoneListViewItem()
        {
            this.InitializeComponent();
        }
    }
}
