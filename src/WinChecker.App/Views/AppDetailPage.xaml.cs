using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using WinChecker.App.ViewModels;
using WinChecker.Core;

namespace WinChecker.App.Views
{
    public sealed partial class AppDetailPage : Page
    {
        public AppDetailViewModel ViewModel { get; }

        public AppDetailPage()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<AppDetailViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is InstalledApp app)
            {
                await ViewModel.InitializeAsync(app);
            }
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
