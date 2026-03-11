using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinChecker.Data;
using WinChecker.Core.Services;
using WinChecker.Core.Repositories;
using WinChecker.Core.Models;
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

        private static IHost? _host;
        public static IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("App is not initialized.");

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = Services.GetService<ILogger<App>>();
            logger?.LogCritical(e.Exception, "Unhandled Exception: {Message}", e.Message);
            
            // Emergency fallback log
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker");
                Directory.CreateDirectory(appDataPath);
                File.AppendAllText(Path.Combine(appDataPath, "critical.log"), $"{DateTime.Now}: {e.Exception}{Environment.NewLine}");
            }
            catch { }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureLogging(builder =>
                    {
                        builder.AddDebug();
                        builder.AddConsole();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker");
                        
                        try { Directory.CreateDirectory(appDataPath); }
                        catch (Exception ex) { Debug.WriteLine($"Failed to create app data directory: {ex}"); }

                        var dbPath = Path.Combine(appDataPath, "winchecker.db");
                        
                        // Configure Options
                        services.Configure<DatabaseOptions>(options => 
                        {
                            options.ConnectionString = $"Data Source={dbPath}";
                        });

                        // Core and Data Services
                        services.AddSingleton<DatabaseMigrator>();
                        services.AddSingleton<IAppRepository, AppRepository>();
                        services.AddSingleton<Win32AppEnumerator>();
                        services.AddSingleton<UwpAppEnumerator>();
                        services.AddSingleton<IIconService, IconService>();
                        services.AddSingleton<IAppScannerService, AppScannerService>();
                        services.AddSingleton<IDllResolver, DllResolver>();
                        services.AddSingleton<IPeParser, PeParser>();

                        // ViewModels
                        services.AddTransient<AppListViewModel>();
                        services.AddTransient<MainPage>();
                    })
                    .Build();

                await _host.StartAsync();

                // Run migration on background thread
                _ = Task.Run(() => 
                {
                    try
                    {
                        Services.GetRequiredService<DatabaseMigrator>().Migrate();
                    }
                    catch (Exception ex)
                    {
                        var logger = Services.GetService<ILogger<App>>();
                        logger?.LogError(ex, "Database migration failed.");
                    }
                });

                _window = new Window();
                _window.ExtendsContentIntoTitleBar = true;
                
                // Safe Backdrop application
                SetMicaBackdrop(_window);

                if (_window.Content is not Frame rootFrame)
                {
                    rootFrame = new Frame();
                    _window.Content = rootFrame;
                }

                rootFrame.Navigate(typeof(ShellPage), e.Arguments);
                _window.Activate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup failed: {ex}");
                try { File.AppendAllText("startup_crash.log", ex.ToString()); } catch { }
            }
        }

        private void SetMicaBackdrop(Window window)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                window.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            }
        }

        public static async Task ShutdownAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
