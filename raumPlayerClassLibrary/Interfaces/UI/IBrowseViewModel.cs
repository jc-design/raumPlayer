using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace raumPlayer.Interfaces
{
    public interface IBrowseViewModel
    {
        ObservableCollection<PivotItemViewModel> PivotItems { get; set; }
        PivotItemViewModel SelectedPivotItem { get; set; }

        ICommand InitializeCommand { get; }
    }
}
