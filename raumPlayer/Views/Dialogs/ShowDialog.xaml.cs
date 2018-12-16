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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace raumPlayer.Views
{
    public sealed partial class ShowDialog : ContentDialog
    {
        public string ContentString
        {
            get { return (string)GetValue(ContentStringProperty); }
            set { SetValue(ContentStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContenString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentStringProperty = DependencyProperty.Register(nameof(ContentString), typeof(string), typeof(ShowDialog), new PropertyMetadata(string.Empty));



        public string SymbolString
        {
            get { return (string)GetValue(SymbolStringProperty); }
            set { SetValue(SymbolStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolStringProperty = DependencyProperty.Register(nameof(SymbolString), typeof(string), typeof(ShowDialog), new PropertyMetadata(string.Empty));

        public ShowDialog()
        {
            this.InitializeComponent();
        }
    }
}
