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
