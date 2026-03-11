using Microsoft.UI.Xaml.Navigation;
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

        public static IServiceProvider Services { get; private set; } = null!;

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker");
                Directory.CreateDirectory(appDataPath);
                var logPath = Path.Combine(appDataPath, "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: Unhandled Exception{Environment.NewLine}{e.Exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch { }
            Debug.WriteLine($"Unhandled Exception: {e.Exception}");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                ConfigureServices();
                Services.GetRequiredService<DatabaseMigrator>().Migrate();

                _window = new Window();
                _window.ExtendsContentIntoTitleBar = true;
                _window.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

                if (_window.Content is not Frame rootFrame)
                {
                    rootFrame = new Frame();
                    _window.Content = rootFrame;
                }

                rootFrame.Navigate(typeof(MainPage), e.Arguments);
                _window.Activate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup failed: {ex}");
                try { File.AppendAllText("startup_crash.log", ex.ToString()); } catch { }
            }
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

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

            Services = services.BuildServiceProvider();
        }
    }
}
