using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.Interfaces
{
    public interface IBrowseViewModel
    {
        ObservableCollection<IPivotItemViewModel> PivotItems { get; set; }
        IPivotItemViewModel SelectedPivotItem { get; set; }
    }
}
