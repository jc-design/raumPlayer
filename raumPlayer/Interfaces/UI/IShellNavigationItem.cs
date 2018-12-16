using raumPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace raumPlayer.Interfaces
{
    public interface IShellNavigationItem
    {
        Visibility SelectedVisibility { get; set; }

        string Label { get; set; }
        string SymbolAsString { get; set; }
        string PageIdentifier { get; set; }
        string SecondPageIdentifier { get; set; }

        bool IsSelected { get; set; }
        bool IsEnabled { get; set; }
        bool HasSecondFunction { get; }

        IShellViewModel Parent { get; }
    }
}
