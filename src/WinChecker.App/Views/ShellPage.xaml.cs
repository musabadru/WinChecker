using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WinChecker.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
            
            this.Loaded += ShellPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;
            RootNavigationView.BackRequested += RootNavigationView_BackRequested;
        }

        private void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppTitleBar != null)
            {
                var app = Application.Current as App;
                if (app?.MainWindow != null)
                {
                    app.MainWindow.SetTitleBar(AppTitleBar);
                }
            }
            // SelectedItem set in ctor doesn't fire SelectionChanged — navigate explicitly
            ContentFrame.Navigate(typeof(MainPage));
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            RootNavigationView.IsBackEnabled = ContentFrame.CanGoBack;
            if (ContentFrame.SourcePageType == typeof(MainPage))
                RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
        }

        private void RootNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Navigate to settings (when implemented)
            }
            else if (args.SelectedItemContainer is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "Apps":
                        ContentFrame.Navigate(typeof(MainPage));
                        break;
                    // Add other cases as pages are implemented
                }
            }
        }
    }
}
