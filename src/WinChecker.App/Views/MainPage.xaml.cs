using WinChecker.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinChecker.Core;

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
            ViewModel = App.Services.GetRequiredService<AppListViewModel>();
            
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Apps.Count == 0)
            {
                await ViewModel.ScanAppsAsync();
            }
        }

        private void OnAppClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is InstalledApp app)
            {
                this.Frame.Navigate(typeof(AppDetailPage), app);
            }
        }
    }
}
