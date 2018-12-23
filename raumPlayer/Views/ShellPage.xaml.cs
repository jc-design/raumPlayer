using System;

using Prism.Windows.Mvvm;

using raumPlayer.ViewModels;

using Windows.UI.Xaml.Controls;

namespace raumPlayer.Views
{
    public sealed partial class ShellPage : Page
    {
        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        public Frame ShellFrame => shellFrame;

        public ShellPage()
        {
            InitializeComponent();
        }

        public async void SetRootFrame(Frame frame)
        {
            shellFrame.Content = frame;
            await ViewModel.InitializeAsync(frame, navigationView);
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // Workaround for Issue https://github.com/Microsoft/WindowsTemplateStudio/issues/2774
            // Using EventTriggerBehavior does not work on WinUI NavigationView ItemInvoked event in Release mode.
            ViewModel.ItemInvokedCommand.Execute(args);
        }
    }
}
