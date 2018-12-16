using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace raumPlayer.UserControls
{
    public sealed partial class TabHeader : UserControl
    {
        public static readonly DependencyProperty SymbolAsStringProperty = DependencyProperty.Register(nameof(SymbolAsString), typeof(string), typeof(TabHeader), new PropertyMetadata(string.Empty));

        public string SymbolAsString
        {
            get { return (string)GetValue(SymbolAsStringProperty); }
            set { SetValue(SymbolAsStringProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(TabHeader), new PropertyMetadata(string.Empty));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public TabHeader()
        {
            this.InitializeComponent();
        }
    }
}
