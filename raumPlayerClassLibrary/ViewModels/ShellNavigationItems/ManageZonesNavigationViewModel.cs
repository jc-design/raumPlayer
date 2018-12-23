using Prism.Events;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;
using Windows.UI.Xaml;

namespace raumPlayer.ViewModels
{
    public class ManageZonesNavigationViewModel : ViewModelBase, IShellNavigationItem
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRaumFeldService raumFeldService;

        private Visibility selectedVisibility = Visibility.Collapsed;
        public Visibility SelectedVisibility
        {
            get { return selectedVisibility; }
            set { SetProperty(ref selectedVisibility, value); }
        }

        private string label;
        public string Label
        {
            get { return label; }
            set { SetProperty(ref label, value); }
        }

        public string SymbolAsString { get; set; }
        public string PageIdentifier { get; set; }
        public string SecondPageIdentifier { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
                SelectedVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        public bool HasSecondFunction { get; }

        public IShellViewModel Parent { get; }

        public ManageZonesNavigationViewModel(IEventAggregator eventAggregatorInstance, IRaumFeldService raumFeldServiceInstance, IShellViewModel shellViewModel, string symbol, string pageIdentifier, string secondPageIdentifier = "")
        {
            eventAggregator = eventAggregatorInstance;
            raumFeldService = raumFeldServiceInstance;
            Parent = shellViewModel;

            Label = string.Empty;
            SymbolAsString = symbol;
            PageIdentifier = pageIdentifier;
            SecondPageIdentifier = secondPageIdentifier;

            IsEnabled = false;
            HasSecondFunction = true;
        }
    }
}
