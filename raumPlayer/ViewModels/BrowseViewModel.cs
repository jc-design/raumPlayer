using Prism.Events;
using Prism.Unity.Windows;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using raumPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Upnp;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace raumPlayer.ViewModels
{
    public class BrowseViewModel : ViewModelBase, IBrowseViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessagingService messagingService;
        private readonly IShellViewModel shellViewModel;

        private ObservableCollection<IPivotItemViewModel> pivotItems;
        public ObservableCollection<IPivotItemViewModel> PivotItems
        {
            get { return pivotItems; }
            set { SetProperty(ref pivotItems, value); }
        }

        private IPivotItemViewModel selectedPivotItem;
        public IPivotItemViewModel SelectedPivotItem
        {
            get { return selectedPivotItem; }
            set
            {
                SetProperty(ref selectedPivotItem, value, () =>
                {
                    shellViewModel.SetBackbuttonVisibility(value.IsGoBackInCacheEnabled);
                    if ((SelectedPivotItem.Elements?.Count() ?? 0) == 0)
                    {
                        SelectedPivotItem.RefreshElementsCommand.Execute(null);
                    }
                });
            }
        }

        public BrowseViewModel(IEventAggregator eventAggregatorInstance, IMessagingService messagingServiceInstance, IShellViewModel shellViewModelInstance)
        {
            eventAggregator = eventAggregatorInstance;
            messagingService = messagingServiceInstance;
            shellViewModel = shellViewModelInstance;

            PivotItems = new ObservableCollection<IPivotItemViewModel>();
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += goBackRequested;
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= goBackRequested;
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void goBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = (SelectedPivotItem.CacheElements?.Count() ?? 0) > 1;
            SelectedPivotItem.GoBackInCache();
        }
    }
}
