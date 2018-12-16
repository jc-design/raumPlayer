using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace raumPlayer.UserControls
{
    public sealed class MarqueeUserControl : Control
    {
        public static readonly DependencyProperty MarqueeDirectionProperty = DependencyProperty.Register(nameof(MarqueeDirection), typeof(MarqueeScrollingDirection), typeof(MarqueeUserControl),new PropertyMetadata(MarqueeScrollingDirection.None));
        public MarqueeScrollingDirection MarqueeDirection
        {
            get { return (MarqueeScrollingDirection)GetValue(MarqueeDirectionProperty); }
            set { SetValue(MarqueeDirectionProperty, value); }
        }

        public static readonly DependencyProperty MarqueeTextProperty = DependencyProperty.Register(nameof(MarqueeText), typeof(string), typeof(MarqueeUserControl), new PropertyMetadata(string.Empty));
        public string MarqueeText
        {
            get { return (string)GetValue(MarqueeTextProperty); }
            set { SetValue(MarqueeTextProperty, value); }
        }

        public MarqueeUserControl()
        {
            this.DefaultStyleKey = typeof(MarqueeUserControl);
            this.SizeChanged += MarqueeUserControl_SizeChanged;
        }

        private Canvas ContentCanvas;
        private TextBlock MarqueeTextBlock;
        private Storyboard storyboard;
        private DoubleAnimation doubleAnimation;

        protected override void OnApplyTemplate()
        {
            MarqueeTextBlock = (TextBlock)GetTemplateChild(nameof(MarqueeTextBlock));
            ContentCanvas = (Canvas)GetTemplateChild(nameof(ContentCanvas));

            if (MarqueeDirection != MarqueeScrollingDirection.None)
            {
                MarqueeTextBlock.SizeChanged += MarqueeUserControl_SizeChanged;

                storyboard = new Storyboard();
                doubleAnimation = new DoubleAnimation();

                doubleAnimation.AutoReverse = true;
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

                if (MarqueeDirection == MarqueeScrollingDirection.FromLeft || MarqueeDirection == MarqueeScrollingDirection.FromRight)
                {
                    Storyboard.SetTargetProperty(doubleAnimation, "(UIElement.RenderTransform).(TranslateTransform.X)");
                }
                if (MarqueeDirection == MarqueeScrollingDirection.FromTop || MarqueeDirection == MarqueeScrollingDirection.FromBottom)
                {
                    Storyboard.SetTargetProperty(doubleAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                }

                Storyboard.SetTarget(doubleAnimation, MarqueeTextBlock);
            }
            else
            {
                (MarqueeTextBlock.RenderTransform as TranslateTransform).X = (ContentCanvas.ActualWidth - MarqueeTextBlock.ActualWidth) / 2;
            }
        }

        private void MarqueeUserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MarqueeDirection != MarqueeScrollingDirection.None)
            {
                bool play = false;

                RectangleGeometry rectangleGeometry = new RectangleGeometry()
                {
                    Rect = new Rect(0, 0, ContentCanvas.ActualWidth, ContentCanvas.ActualHeight)
                };
                ContentCanvas.Clip = rectangleGeometry;

                storyboard.Stop();
                storyboard.Children.Clear();

                switch (MarqueeDirection)
                {
                    case MarqueeScrollingDirection.FromLeft:
                        doubleAnimation.From = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth ? ContentCanvas.ActualWidth - MarqueeTextBlock.ActualWidth : 0;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth ? ((MarqueeTextBlock.ActualWidth - ContentCanvas.ActualWidth) / 10) +1 : 0));

                        play = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth;
                        break;
                    case MarqueeScrollingDirection.FromRight:
                        doubleAnimation.From = 0;
                        doubleAnimation.To = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth ? ContentCanvas.ActualWidth - MarqueeTextBlock.ActualWidth : 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth ? ((MarqueeTextBlock.ActualWidth - ContentCanvas.ActualWidth) / 10) + 1 : 0));

                        play = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth;
                        break;
                    case MarqueeScrollingDirection.FromTop:

                        play = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth;
                        break;
                    case MarqueeScrollingDirection.FromBottom:

                        play = MarqueeTextBlock.ActualWidth > ContentCanvas.ActualWidth;
                        break;
                    case MarqueeScrollingDirection.None:

                        play = false;
                        break;
                    default:
                        break;
                }

                if (play)
                {
                    storyboard.Children.Add(doubleAnimation);
                    storyboard.Begin();
                }
                else
                {
                    (MarqueeTextBlock.RenderTransform as TranslateTransform).X = (ContentCanvas.ActualWidth - MarqueeTextBlock.ActualWidth) / 2;
                }
            }
        }
    }
}
