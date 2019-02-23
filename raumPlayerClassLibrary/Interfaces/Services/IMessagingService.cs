using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace raumPlayer.Interfaces
{
    public interface IMessagingService
    {
        Task ShowStoreRatingDialogAsync();
        Task ShowDialogAsync(string content, string title = null);
        Task ShowErrorDialogAsync(Exception content);
        Task ShowErrorDialogAsync(string content);
        Task ShowActionDialogAsync(string content, ICommand okCallback, string title = null, string okButtonText = "OK", string cancelButtonText = "Cancel");

        string AppName { get; }
        Uri AppLogo { get; }
        string AppVersion { get; }
        string AppDisplayName { get; }
        string AppPublisher { get; }
    }
}
