using System;
using raumPlayer.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class PivotItemView : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PivotItemViewModel), typeof(PivotItemView), new PropertyMetadata(null, new PropertyChangedCallback(OnViewModelChanged)));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        public PivotItemViewModel ViewModel
        {
            get { return (PivotItemViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }

        #endregion

        public PivotItemView()
        {
            this.InitializeComponent();
        }

        private void ElementControl_ItemTapped(object sender, Services.EventArgs<ElementBase> e)
        {
            int i = 0;
        }
    }
}
