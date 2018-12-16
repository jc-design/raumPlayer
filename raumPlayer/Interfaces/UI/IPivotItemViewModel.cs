using raumPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace raumPlayer.Interfaces
{
    public interface IPivotItemViewModel
    {
        IPivotItemViewModel ViewModel { get; }

        bool IsGoBackInCacheEnabled { get; set; }
        bool IsScanning { get; set; }

        string PivotSymbol { get; set; }
        string PivotLabel { get; set; }

        Visibility IsScanningVisibility { get; set; }
        Visibility GroupVisibility { get; set; }
        Visibility ListVisibility { get; set; }

        ObservableCollection<IElement> Elements { get; set; }
        ObservableCollection<IElement> FilteredElements { get; set; }
        //ObservableCollection<ElementGroup> GroupedElements { get; set; }

        ObservableCollection<IElement> CacheElements { get; set; }
        IElement LastCacheElement { get; set; }
        void GoBackInCache();

        ICommand InitializeCommand { get; }
        ICommand RefreshElementsCommand { get; }
        ICommand QuerySubmittedFilterCommand { get; }
        ICommand QuerySubmittedSearchCommand { get; }
    }
}
