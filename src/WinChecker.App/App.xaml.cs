using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WinChecker.Data;
using WinChecker.Core.Services;
using WinChecker.Core.Repositories;
using WinChecker.Data.Repositories;
using WinChecker.Enumeration;
using WinChecker.PE;
using System.IO;

namespace WinChecker.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window window = Window.Current;

        public static IHost Host { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Core and Data Services
                    var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker");
                    Directory.CreateDirectory(appDataPath);
                    var dbPath = Path.Combine(appDataPath, "winchecker.db");
                    var connectionString = $"Data Source={dbPath}";
                    
                    services.AddSingleton(new DatabaseMigrator(connectionString));
                    services.AddSingleton<IAppRepository>(new AppRepository(connectionString));
                    services.AddSingleton<Win32AppEnumerator>();
                    services.AddSingleton<UwpAppEnumerator>();
                    services.AddSingleton<IAppScannerService, AppScannerService>();
                    services.AddSingleton<IPeParser, PeParser>();
                    
                    // ViewModels
                    
                    // Views
                    services.AddTransient<MainPage>();
                })
                .Build();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Host.Services.GetRequiredService<DatabaseMigrator>().Migrate();

            window ??= new Window();

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                window.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            window.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
