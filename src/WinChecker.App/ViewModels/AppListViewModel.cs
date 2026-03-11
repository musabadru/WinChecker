using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinChecker.Core;
using WinChecker.Core.Services;

namespace WinChecker.App.ViewModels;

public partial class AppListViewModel : ObservableObject
{
    private readonly IAppScannerService _scannerService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<InstalledApp> Apps { get; } = new();

    public AppListViewModel(IAppScannerService scannerService)
    {
        _scannerService = scannerService;
    }

    [RelayCommand]
    public async Task ScanAppsAsync()
    {
        IsLoading = true;
        Apps.Clear();

        try
        {
            var results = await _scannerService.ScanAllAppsAsync();
            foreach (var app in results)
            {
                Apps.Add(app);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
