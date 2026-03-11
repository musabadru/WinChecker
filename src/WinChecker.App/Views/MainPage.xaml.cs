using WinChecker.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinChecker.App.Views
{
    /// <summary>
    /// A simple page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        public AppListViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<AppListViewModel>();
            
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the drag region for the custom title bar
            if (AppTitleBar != null)
            {
                var app = Application.Current as App;
                if (app?.MainWindow != null)
                {
                    app.MainWindow.SetTitleBar(AppTitleBar);
                }
            }

            if (ViewModel.Apps.Count == 0)
            {
                await ViewModel.ScanAppsAsync();
            }
        }
    }
}
