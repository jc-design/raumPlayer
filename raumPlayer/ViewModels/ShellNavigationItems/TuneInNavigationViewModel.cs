using System;
using Prism.Events;
using Prism.Mvvm;
using Prism.Windows.Mvvm;
using raumPlayer.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using raumPlayer.PrismEvents;
using Microsoft.Toolkit.Uwp.Helpers;
using raumPlayer.Models;
using System.Windows.Input;
using Prism.Commands;

namespace raumPlayer.ViewModels
{
    public class TuneInNavigationViewModel : ViewModelBase, IShellNavigationItem
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

        public TuneInNavigationViewModel(IEventAggregator eventAggregatorInstance, IRaumFeldService raumFeldServiceInstance, IShellViewModel shellViewModel, string label, string symbol, string pageIdentifier)
        {
            eventAggregator = eventAggregatorInstance;
            raumFeldService = raumFeldServiceInstance;
            Parent = shellViewModel;

            eventAggregator.GetEvent<SystemUpdateIDChangedEvent>().Subscribe(onSystemUpdateIDChanged,ThreadOption.UIThread);

            Label = label;
            SymbolAsString = symbol;
            PageIdentifier = pageIdentifier;
            SecondPageIdentifier = string.Empty;

            HasSecondFunction = false;

            setIsEnabledCommand = new DelegateCommand<object>(async (param) => { IsEnabled = await raumFeldService.GetTuneInState(); });
            setIsEnabledCommand.Execute(null);
        }

        private ICommand setIsEnabledCommand;

        private async void onSystemUpdateIDChanged(RaumFeldEvent args)
        {
            IsEnabled = await raumFeldService.GetTuneInState();
        }
    }
}
