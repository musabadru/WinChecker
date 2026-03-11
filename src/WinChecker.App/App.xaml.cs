using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WinChecker.Data;
using WinChecker.Core.Services;
using WinChecker.Core.Repositories;
using WinChecker.Data.Repositories;
using WinChecker.Enumeration;
using WinChecker.PE;
using WinChecker.App.ViewModels;
using System.IO;
using System.Diagnostics;

namespace WinChecker.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public Window? MainWindow => _window;

        public static IHost Host { get; private set; } = null!;

        public App()
        {
            this.UnhandledException += App_UnhandledException;
            this.InitializeComponent();

            try
            {
                Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker");
                        Directory.CreateDirectory(appDataPath);
                        var dbPath = Path.Combine(appDataPath, "winchecker.db");
                        var connectionString = $"Data Source={dbPath}";
                        
                        services.AddSingleton(new DatabaseMigrator(connectionString));
                        services.AddSingleton<IAppRepository>(new AppRepository(connectionString));
                        services.AddSingleton<Win32AppEnumerator>();
                        services.AddSingleton<UwpAppEnumerator>();
                        services.AddSingleton<IAppScannerService, AppScannerService>();
                        services.AddSingleton<IDllResolver, DllResolver>();
                        services.AddSingleton<IPeParser, PeParser>();
                        
                        services.AddTransient<AppListViewModel>();
                        services.AddTransient<MainPage>();
                    })
                    .Build();
            }
            catch (Exception ex)
            {
                LogFatalError("Host initialization failed", ex);
                throw;
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogFatalError("Unhandled Exception", e.Exception);
        }

        private void LogFatalError(string context, Exception ex)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker", "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {context}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch { }
            Debug.WriteLine($"{context}: {ex}");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Host.Services.GetRequiredService<DatabaseMigrator>().Migrate();
            }
            catch (Exception ex)
            {
                LogFatalError("Migration failed", ex);
            }

            _window = new Window();
            
            // Try to set modern styling, but wrap in try-catch as it can be picky on some systems
            try
            {
                _window.ExtendsContentIntoTitleBar = true;
                _window.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set modern window styling: {ex}");
            }

            if (_window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                _window.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            _window.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LogFatalError("Navigation failed", e.Exception);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
