using Prism.Commands;
using Prism.Windows.AppModel;
using raumPlayer.Helpers;
using raumPlayer.Interfaces;
using raumPlayer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.System;

namespace raumPlayer.Services
{
    /// <summary>
    /// The MessagingService class provides a way to show different dialogs for in app user notification
    /// Important Note: https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Popups.MessageDialog
    /// You should use MessageDialog only when you are upgrading a Universal Windows 8 app that uses MessageDialog, and need to minimize changes.For new apps in Windows 10, we recommend using the ContentDialog control instead.
    /// </summary>
    public class MessagingService : IMessagingService
    {
        private const string SYMBOL_QUESTION = "\uE11B";
        private const string SYMBOL_VERBOSE = "\uE939";
        private const string SYMBOL_INFO = "\uEC24";
        private const string SYMBOL_WARNING = "\uE814";
        private const string SYMBOL_ERROR = "\uEB90";
        private const string SYMBOL_CRITICAL = "\uE945";

        public MessagingService()
        {
        }

        /// <summary>
        /// Shows a dialog for user feedback
        /// </summary>
        /// <param name="content">Any string to display</param>
        /// <param name="okCallback">ICommand for user feedback</param>
        /// <param name="title">Title of dialog</param>
        /// <param name="okButtonText">Text for Primary button</param>
        /// <param name="cancelButtonText">Text for Secondary button</param>
        /// <returns></returns>
        public async Task ShowActionDialogAsync(string content, ICommand okCallback, string title = null, string okButtonText = "DialogOk", string cancelButtonText = "DialogCancel")
        {
            var dialog = new ShowDialog
            {
                SymbolString = SYMBOL_QUESTION,
                ContentString = content.GetLocalized(),
                PrimaryButtonText = okButtonText.GetLocalized(),
                PrimaryButtonCommand = okCallback,
                SecondaryButtonText = cancelButtonText.GetLocalized(),
                Title = title.GetLocalized(),
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a dialog. Primarybutton set
        /// </summary>
        /// <param name="content">Any string to display</param>
        /// <param name="title">Title of Dialog</param>
        /// <returns></returns>
        public async Task ShowDialogAsync(string content, string title = null)
        {
            var dialog = new ShowDialog
            {
                SymbolString = SYMBOL_INFO,
                ContentString = content.GetLocalized(),
                PrimaryButtonText = "DialogContinue".GetLocalized(),
                Title = title.GetLocalized(),
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows an error dialog. Primarybutton and title set
        /// </summary>
        /// <param name="exception">Exception to show</param>
        /// <returns></returns>
        public async Task ShowErrorDialogAsync(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}: {1}", "DialogErrorMessage".GetLocalized(), exception.Message));
            sb.AppendLine(string.Format("{0}: 0x{1:X})", "DialogUnhandledError".GetLocalized(), exception.HResult));

            var dialog = new ShowDialog
            {
                SymbolString = SYMBOL_ERROR,
                ContentString = sb.ToString(),
                PrimaryButtonText = "DialogDismiss".GetLocalized(),
                Title = "DialogError".GetLocalized(),
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows an error dialog. Primarybutton and title set
        /// </summary>
        /// <param name="exception">Exception to show</param>
        /// <returns></returns>
        public async Task ShowErrorDialogAsync(string error)
        {
            var dialog = new ShowDialog
            {
                SymbolString = SYMBOL_ERROR,
                ContentString = error,
                PrimaryButtonText = "DialogDismiss".GetLocalized(),
                Title = "DialogError".GetLocalized(),
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Ask the user to rate the app. Positiv answer will open the storeapp
        /// </summary>
        /// <returns></returns>
        public async Task ShowStoreRatingDialogAsync()
        {
            var dialog = new ShowDialog
            {
                SymbolString = SYMBOL_VERBOSE,
                ContentString = "DialogRateAppContent".GetLocalized(),
                PrimaryButtonText = "DialogYes".GetLocalized(),
                PrimaryButtonCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri($"ms-windows-store:REVIEW?PFN={Package.Current.Id.FamilyName}"))),
                SecondaryButtonText = "DialogNotYet".GetLocalized(),
                Title = "DialogRateAppTitle".GetLocalized(),
            };
            await dialog.ShowAsync();
        }

        public string AppName
        {
            get { return Package.Current.Id.Name; }
        }

        public Uri AppLogo
        {
            get { return Package.Current.Logo; }
        }

        public string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public string AppDisplayName => Package.Current.DisplayName;

        public string AppPublisher => Package.Current.PublisherDisplayName;
    }
}
