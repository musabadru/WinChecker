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
using Serilog;
using Serilog.Events;

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
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "WinChecker", "logs", "winchecker-.log");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
#if DEBUG
                    .WriteTo.Debug()
#endif
                    .CreateLogger();

                _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .UseSerilog()
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
                        services.AddTransient<AppDetailViewModel>();
                        services.AddTransient<MainPage>();
                        services.AddTransient<AppDetailPage>();
                    })
                    .Build();

                await _host.StartAsync();

                // Run migration on background thread; await so the DB is ready before first navigation
                var sw = Stopwatch.StartNew();
                try
                {
                    await Task.Run(() => Services.GetRequiredService<DatabaseMigrator>().Migrate());
                    Log.Information("Migration completed in {Elapsed}ms", sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Migration failed after {Elapsed}ms", sw.ElapsedMilliseconds);
                }

                _window = new Window();
                _window.ExtendsContentIntoTitleBar = true;
                // Dispose the host when the window closes
                _window.Closed += async (_, _) => await ShutdownAsync();
                
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
#if DEBUG
                throw;  // surface startup failures immediately during development
#endif
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
            Log.CloseAndFlush();
        }
    }
}
