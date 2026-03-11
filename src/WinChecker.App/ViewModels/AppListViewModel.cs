using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinChecker.Core;
using WinChecker.Core.Services;

namespace WinChecker.App.ViewModels;

public partial class AppListViewModel : ObservableObject
{
    private readonly IAppScannerService _scannerService;
    private readonly ILogger<AppListViewModel> _logger;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<InstalledApp> Apps { get; } = new();

    public AppListViewModel(IAppScannerService scannerService, ILogger<AppListViewModel> logger)
    {
        _scannerService = scannerService;
        _logger = logger;
    }

    [RelayCommand]
    public async Task ScanAppsAsync()
    {
        IsLoading = true;
        Apps.Clear();

        var sw = Stopwatch.StartNew();

        // Phase 1: instant cache load
        _logger.LogInformation("Phase 1: loading cached apps");
        try
        {
            foreach (var app in await _scannerService.GetCachedAppsAsync())
                Apps.Add(app);
            _logger.LogInformation("Phase 1 complete: {Count} cached apps in {Elapsed}ms", Apps.Count, sw.ElapsedMilliseconds);
        }
        catch (Exception ex) { _logger.LogError(ex, "Scan phase failed"); }

        // Phase 2: live scan replaces stale results
        _logger.LogInformation("Phase 2: starting live scan");
        sw.Restart();
        try
        {
            Apps.Clear();
            var count = 0;
            await foreach (var app in _scannerService.ScanAllAppsAsync())
            {
                Apps.Add(app);
                count++;
            }
            _logger.LogInformation("Phase 2 complete: {Count} apps in {Elapsed}ms", count, sw.ElapsedMilliseconds);
        }
        catch (Exception ex) { _logger.LogError(ex, "Scan phase failed"); }
        finally { IsLoading = false; }
    }
}
