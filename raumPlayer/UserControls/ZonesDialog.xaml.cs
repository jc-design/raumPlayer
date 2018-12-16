using raumPlayer.Helpers;
using raumPlayer.ViewModels;
using raumPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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

namespace raumPlayer.UserControls
{
    public sealed partial class ZonesDialog : ContentDialog
    {
        public ObservableCollection<ZoneViewModel> ZoneViewModels => Shell.Instance.ViewModel.ZoneViewModels;

        public ZonesDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        #region ICommand

        /// <summary>
        /// Set new active Zone
        /// </summary>
        private ICommand checkedCommand;
        public ICommand CheckedCommand
        {
            get
            {
                if (checkedCommand == null)
                {
                    checkedCommand = new RelayCommand<ZoneViewModel>(Checked);
                }

                return checkedCommand;
            }
        }
        public void Checked(ZoneViewModel obj)
        {
            obj.IsActive = true;
        }

        /// <summary>
        /// Set initial value to ComboBox
        /// </summary>
        private ICommand comboBoxLoadedCommand;
        public ICommand ComboBoxLoadedCommand
        {
            get
            {
                if (comboBoxLoadedCommand == null)
                {
                    comboBoxLoadedCommand = new RelayCommand<ComboBox>(ComboBoxLoaded);
                }

                return comboBoxLoadedCommand;
            }
        }
        public void ComboBoxLoaded(ComboBox obj)
        {
            bool handled = false;
            for (int i = 0; i < (ZoneViewModels?.Count() ?? 0); i++)
            {
                if (ZoneViewModels[0].IsActive)
                {
                    obj.SelectedIndex = i;
                    handled = true;
                    break;
                }
            }

            if (!handled && (ZoneViewModels?.Count ?? 0) > 0)
            {
                obj.SelectedIndex = 0;
                ZoneViewModels[0].IsActive = true;
            }
        }

        #endregion
    }
}
