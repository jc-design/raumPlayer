using JCDesign.Services;
using raumPlayer.Models;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class GroupedBrowserControl : UserControl
    {
        /// <summary>
        /// Event occurs when FolderItem is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> FolderItemClicked;

        /// <summary>
        /// Event occurs when NewQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> NewQueueClicked;

        /// <summary>
        /// Event occurs when AddQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> AddQueueClicked;

        /// <summary>
        /// Event occurs when RemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> RemQueueClicked;

        /// <summary>
        /// Event occurs when FavQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> FavQueueClicked;

        /// <summary>
        /// Event occurs when FavRemQueue is clicked
        /// Param: EventArgs <Element>element</Element>
        /// </summary>
        public event EventHandler<EventArgs<Element>> FavRemQueueClicked;

        public static readonly DependencyProperty GroupSourceProperty = DependencyProperty.Register(nameof(GroupSource), typeof(object), typeof(GroupedBrowserControl), null);
        public object GroupSource
        {
            get { return (object)GetValue(GroupSourceProperty); }
            set { SetValue(GroupSourceProperty, value); }
        }

        public GroupedBrowserControl()
        {
            this.InitializeComponent();
        }

        private void gridViewBrowse_ItemClick(object sender, ItemClickEventArgs arg)
        {
            if (arg.ClickedItem is Element element && element.IsFolder)
            {
                FolderItemClicked?.Invoke(this, new EventArgs<Element>(element));
            }
        }

        private void UserControl_OnNewQueueClicked(object sender, EventArgs<Element> arg)
        {
            NewQueueClicked?.Invoke(this, new EventArgs<Element>(arg.Value));
        }

        private void UserControl_OnAddQueueClicked(object sender, EventArgs<Element> arg)
        {
            AddQueueClicked?.Invoke(this, new EventArgs<Element>(arg.Value));
        }

        private void UserControl_OnRemQueueClicked(object sender, EventArgs<Element> arg)
        {
            RemQueueClicked?.Invoke(this, new EventArgs<Element>(arg.Value));
        }

        private void UserControl_OnFavQueueClicked(object sender, EventArgs<Element> arg)
        {
            FavQueueClicked?.Invoke(this, new EventArgs<Element>(arg.Value));
        }

        private void UserControl_OnFavRemQueueClicked(object sender, EventArgs<Element> arg)
        {
            FavRemQueueClicked?.Invoke(this, new EventArgs<Element>(arg.Value));
        }
    }

    #region DataTemplateSelector
    public class ElementGroupTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StandardTemplate { get; set; }
        public DataTemplate HideTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var template = item as ICollectionViewGroup;

            if (container is SelectorItem selectorItem)
            {
                // Disable Item if necessary
                selectorItem.IsHitTestVisible = ((template?.GroupItems?.Count() ?? 0) > 0);               
            }

            if ((template?.GroupItems?.Count() ?? 0) == 0) { return HideTemplate; }
            else { return StandardTemplate; }
        }
    }

    #endregion
}
