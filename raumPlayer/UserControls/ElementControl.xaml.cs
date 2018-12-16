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

        private DropShadow dropShadow;
        private SpriteVisual shadowVisual;
        const float initialShadowBlurRadius = 15.0f;
        const float initialShadowOpacity = 0.5f;

        #endregion

        /// <summary>
        /// Event occurs when NewQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> NewQueueClicked;

        /// <summary>
        /// Event occurs when AddQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> AddQueueClicked;

        /// <summary>
        /// Event occurs when RemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> RemQueueClicked;

        /// <summary>
        /// Event occurs when FavQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> FavQueueClicked;

        /// <summary>
        /// Event occurs when FavRemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<ElementBase>> FavRemQueueClicked;

        #region DependencyProperties

        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(nameof(Element), typeof(ElementBase), typeof(ElementControl), new PropertyMetadata(null));
        public ElementBase Element
        {
            get { return (ViewModels.ElementBase)GetValue(ElementProperty); }
            set
            {
                SetValue(ElementProperty, value);
                setInitValues();
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
            SizeChanged += CompositionShadow_SizeChanged;
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                UpdateShadowSize();
            };

            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            shadowVisual = compositor.CreateSpriteVisual();
            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 10.0f;

            if (RequestedTheme == ElementTheme.Light)
            {
                dropShadow.Color = (Color)Prism.Unity.Windows.PrismUnityApplication.Current.Resources["AppDarkShadeColor"];
            }
            else
            {
                dropShadow.Color = Color.FromArgb(170, 242, 242, 242);
            }
            dropShadow.Offset = new Vector3(0, 0, 0);
            shadowVisual.Shadow = dropShadow;

            // SetElementChildVisual on the control itself ("this") would result in the shadow
            // rendering on top of the content. To avoid this, CompositionShadow contains a Border
            // (to host the shadow) and a ContentPresenter (to hose the actual content, "CastingElement").
            ElementCompositionPreview.SetElementChildVisual(shadowBorder, shadowVisual);

            rootPanel.PointerEntered += Content_PointerEntered;
            rootPanel.PointerExited += Content_PointerExited;

            SetupAnimations();

            var content = ElementCompositionPreview.GetElementVisual(shadowBorder);
            content.Scale = new Vector3(0.5f, 0.5f, 25.0f);
            Canvas.SetZIndex(shadowBorder, 0);
        }

        #region Private Methods

        private void setInitValues()
        {
            switch (Element.Id)
            {
                case var checkID when checkID.Contains("0/Playlists/MyPlaylists/"):
                    buttonAdd.Visibility = Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Visible;
                    buttonFav.Visibility = Visibility.Collapsed;
                    buttonFavRem.Visibility = Visibility.Collapsed;
                    break;
                case var checkID when checkID.Contains("0/Playlists"):
                    buttonAdd.Visibility = Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Collapsed;
                    buttonFav.Visibility = Visibility.Collapsed;
                    buttonFavRem.Visibility = Visibility.Collapsed;
                    break;
                case var checkID when checkID.Contains("0/Favorites/MyFavorites/"):
                    buttonAdd.Visibility = Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Collapsed;
                    buttonFav.Visibility = Visibility.Collapsed;
                    buttonFavRem.Visibility = Visibility.Visible;
                    break;
                case var checkID when checkID.Contains("0/Favorites"):
                    buttonAdd.Visibility = Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Collapsed;
                    buttonFav.Visibility = Visibility.Collapsed;
                    buttonFavRem.Visibility = Visibility.Collapsed;
                    break;
                case null:
                    buttonAdd.Visibility = Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Collapsed;
                    buttonFav.Visibility = Visibility.Collapsed;
                    buttonFavRem.Visibility = Visibility.Collapsed;
                    break;
                default:
                    buttonAdd.Visibility = Element.IsPlayable ? Visibility.Visible : Visibility.Collapsed;
                    buttonRem.Visibility = Visibility.Collapsed;
                    buttonFav.Visibility = Visibility.Visible;
                    buttonFavRem.Visibility = Visibility.Collapsed;
                    break;
            }

            //if (DataElement.IsFolder)
            //{
            //    borderNumber.Visibility = DataElement.ChildCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            //    textBlockNumber.Text = DataElement.ChildCount.ToString();
            //    textBlockLine1.MarqueeText = DataElement?.Album ?? string.Empty;
            //    textBlockLine2.MarqueeText = string.Format("{0} - {1}", DataElement?.Artist ?? string.Empty, DataElement?.Genre ?? string.Empty); 
            //}
            //else
            //{
            //    borderNumber.Visibility = DataElement.OriginalTrackNumber == 0 ? Visibility.Collapsed : Visibility.Visible;
            //    textBlockNumber.Text = DataElement.OriginalTrackNumber.ToString();
            //    textBlockLine1.MarqueeText = DataElement?.Album ?? string.Empty;
            //    textBlockLine2.MarqueeText = string.Format("{0} - {1}", DataElement?.Artist ?? string.Empty, DataElement?.Genre ?? string.Empty);
            //}
        }

        #endregion

        private void buttonNew_Click(object sender, RoutedEventArgs arg)
        {
            this.NewQueueClicked?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs arg)
        {
            this.AddQueueClicked?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonRem_Click(object sender, RoutedEventArgs arg)
        {
            this.RemQueueClicked?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonFav_Click(object sender, RoutedEventArgs arg)
        {
            this.FavQueueClicked?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void buttonFavRem_Click(object sender, RoutedEventArgs arg)
        {
            this.FavRemQueueClicked?.Invoke(this, new EventArgs<ElementBase>(Element));
        }

        private void CompositionShadow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShadowSize();
        }

        private void UpdateShadowSize()
        {
            Vector2 newSize = new Vector2((float)shadowBorder.ActualWidth, (float)shadowBorder.ActualHeight);
            shadowVisual.Size = newSize;
        }

        private void SetupAnimations()
        {
            var content = ElementCompositionPreview.GetElementVisual(shadowBorder);

            var compositor = content.Compositor;
            var implicitAnimationVisual = compositor.CreateImplicitAnimationCollection();

            //Scale Animation Shadow 
            var shadowScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            shadowScaleAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            shadowScaleAnimation.Duration = TimeSpan.FromSeconds(1);
            shadowScaleAnimation.Target = "Scale";

            implicitAnimationVisual["Scale"] = shadowScaleAnimation;

            content.Properties.ImplicitAnimations = implicitAnimationVisual;
        }

        private void Content_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var content = ElementCompositionPreview.GetElementVisual(shadowBorder);
            content.Scale = new Vector3(0.5f, 0.5f, 25.0f);
            Canvas.SetZIndex(shadowBorder, 0);
        }

        private void Content_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var content = ElementCompositionPreview.GetElementVisual(shadowBorder);
            content.CenterPoint = new Vector3((float)rootPanel.ActualWidth / 2, (float)rootPanel.ActualHeight / 2, 0f);
            float xScale = (float)(rootPanel.ActualWidth + 8) / (float)rootPanel.ActualWidth;
            float yScale = (float)(rootPanel.ActualHeight + 8) / (float)rootPanel.ActualHeight;
            content.Scale = new Vector3(xScale, yScale, 1.0f);
            Canvas.SetZIndex(shadowBorder, 0);
        }
    }
}
