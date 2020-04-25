using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Prism.Commands;
using raumPlayer.Interfaces;
using raumPlayer.Models;
using raumPlayer.Services;
using raumPlayer.ViewModels;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class ElementControl : UserControl
    {
        #region Private Properties

        #endregion

        /// <summary>
        /// Event occurs when image is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> ItemTapped;

        /// <summary>
        /// Event occurs when Browse is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> BrowseTapped;

        /// <summary>
        /// Event occurs when NewQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> NewQueueTapped;

        /// <summary>
        /// Event occurs when AddQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> AddQueueTapped;

        /// <summary>
        /// Event occurs when RemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> RemQueueTapped;

        /// <summary>
        /// Event occurs when FavQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> FavQueueTapped;

        /// <summary>
        /// Event occurs when FavRemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> FavRemQueueTapped;

        #region DependencyProperties

        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(nameof(Element), typeof(ElementBase), typeof(ElementControl), new PropertyMetadata(null));
        public ElementBase Element
        {
            get { return (ViewModels.ElementBase)GetValue(ElementProperty); }
            set
            {
                SetValue(ElementProperty, value);
            }
        }

        public static readonly DependencyProperty ImageUriProperty = DependencyProperty.Register(nameof(ImageUri), typeof(string), typeof(ElementControl), new PropertyMetadata(string.Empty));
        public string ImageUri
        {
            get { return (string)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ElementControl), new PropertyMetadata(string.Empty));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty Line1Property = DependencyProperty.Register(nameof(Line1), typeof(string), typeof(ElementControl), new PropertyMetadata(string.Empty));
        public string Line1
        {
            get { return (string)GetValue(Line1Property); }
            set { SetValue(Line1Property, value); }
        }

        public static readonly DependencyProperty Line2Property = DependencyProperty.Register(nameof(Line2), typeof(string), typeof(ElementControl), new PropertyMetadata(string.Empty));
        public string Line2
        {
            get { return (string)GetValue(Line2Property); }
            set { SetValue(Line2Property, value); }
        }

        public static readonly DependencyProperty VisibleCountProperty = DependencyProperty.Register(nameof(VisibleCount), typeof(string), typeof(ElementControl), new PropertyMetadata(string.Empty));
        public string VisibleCount
        {
            get { return (string)GetValue(VisibleCountProperty); }
            set { SetValue(VisibleCountProperty, value); }
        }

        public static readonly DependencyProperty VisibleCountVisibilityProperty = DependencyProperty.Register(nameof(VisibleCountVisibility), typeof(Visibility), typeof(ElementControl), new PropertyMetadata(Visibility.Collapsed));
        public Visibility VisibleCountVisibility
        {
            get { return (Visibility)GetValue(VisibleCountVisibilityProperty); }
            set { SetValue(VisibleCountVisibilityProperty, value); }
        }

        #endregion

        public ElementControl()
        {
            this.InitializeComponent();
        }

        #region Private Methods

        public Visibility AddQueueButtonVisibility(string id, bool isplayable)
        {
            if (id.Contains("0/Playlists/") || id.Contains("0/Favorites/"))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return isplayable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility RemQueueButtonVisibility(string id, bool isplayable)
        {
            if (id.Contains("0/Playlists/MyPlaylists/"))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public Visibility FavAddQueueButtonVisibility(string id, bool isplayable)
        {
            if (id.Contains("0/Playlists/") || id.Contains("0/Favorites/"))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public Visibility FavRemQueueButtonVisibility(string id, bool isplayable)
        {
            if (id.Contains("0/Playlists/MyFavorites/"))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public string GetItemTappedIcon(bool isplayable)
        {
            if (isplayable)
            {
                return "\uEDB5";
            }
            else
            {
                return "\uED25";
            }
        }


        #endregion

        private void image_Tapped(object sender, RoutedEventArgs arg)
        {
            this.ItemTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonBrowse_Tapped(object sender, RoutedEventArgs arg)
        {
            this.BrowseTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonNew_Tapped(object sender, RoutedEventArgs arg)
        {
            this.NewQueueTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonAdd_Tapped(object sender, RoutedEventArgs arg)
        {
            this.AddQueueTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonRem_Tapped(object sender, RoutedEventArgs arg)
        {
            this.RemQueueTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonFav_Tapped(object sender, RoutedEventArgs arg)
        {
            this.FavQueueTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonFavRem_Tapped(object sender, RoutedEventArgs arg)
        {
            this.FavRemQueueTapped?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        public void panelData_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                imageAlbumArt.Height = e.NewSize.Width;
            }
        }
    }
}
