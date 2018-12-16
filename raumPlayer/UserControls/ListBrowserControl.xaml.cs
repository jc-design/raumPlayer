using raumPlayer.Services;
using raumPlayer.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using raumPlayer.Interfaces;
using raumPlayer.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class ListBrowserControl : UserControl
    {
        /// <summary>
        /// Event occurs when FolderItem is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> FolderItemClicked;

        /// <summary>
        /// Event occurs when NewQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> NewQueueClicked;

        /// <summary>
        /// Event occurs when AddQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> AddQueueClicked;

        /// <summary>
        /// Event occurs when RemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> RemQueueClicked;

        /// <summary>
        /// Event occurs when FavQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> FavQueueClicked;

        /// <summary>
        /// Event occurs when FavRemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<IElement>> FavRemQueueClicked;

        public static readonly DependencyProperty ListSourceProperty = DependencyProperty.Register(nameof(ListSource), typeof(object), typeof(ListBrowserControl), null);
        public object ListSource
        {
            get { return (object)GetValue(ListSourceProperty); }
            set { SetValue(ListSourceProperty, value); }
        }

        public ListBrowserControl()
        {
            this.InitializeComponent();
        }

        private void gridViewBrowse_ItemClick(object sender, ItemClickEventArgs arg)
        {
            if (arg.ClickedItem is ElementContainer element)
            {
                FolderItemClicked?.Invoke(this, new EventArgs<IElement>(element));
            }
        }

        private void UserControl_OnNewQueueClicked(object sender, EventArgs<IElement> arg)
        {
            NewQueueClicked?.Invoke(this, new EventArgs<IElement>(arg.Value));
        }

        private void UserControl_OnAddQueueClicked(object sender, EventArgs<IElement> arg)
        {
            AddQueueClicked?.Invoke(this, new EventArgs<IElement>(arg.Value));
        }

        private void UserControl_OnRemQueueClicked(object sender, EventArgs<IElement> arg)
        {
            RemQueueClicked?.Invoke(this, new EventArgs<IElement>(arg.Value));
        }

        private void UserControl_OnFavQueueClicked(object sender, EventArgs<IElement> arg)
        {
            FavQueueClicked?.Invoke(this, new EventArgs<IElement>(arg.Value));
        }

        private void UserControl_OnFavRemQueueClicked(object sender, EventArgs<IElement> arg)
        {
            FavRemQueueClicked?.Invoke(this, new EventArgs<IElement>(arg.Value));
        }
    }
}
