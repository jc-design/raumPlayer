using raumPlayer.Models;
using raumPlayer.ViewModels;
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
        PivotItemViewModel ViewModel { get; }

        bool IsGoBackInCacheEnabled { get; set; }
        bool IsScanning { get; set; }

        string PivotSymbol { get; set; }
        string PivotLabel { get; set; }
        string FilterCriteria { get; set; }

        List<ElementBase> RawElements { get; set; }
        ObservableCollection<ElementBase> Elements { get; set; }

        ObservableCollection<ElementBase> CacheElements { get; set; }
        ElementBase LastCacheElement { get; set; }
        void GoBackInCache();

        ICommand InitializeCommand { get; }
        ICommand RefreshElementsCommand { get; }
        ICommand QuerySubmittedFilterCommand { get; }
        ICommand QuerySubmittedSearchCommand { get; }
        ICommand ItemTappedCommand { get;}
    }
}
