using Microsoft.Extensions.Logging;
using WinChecker.Core;
using WinChecker.Core.Repositories;
using WinChecker.Core.Services;

namespace WinChecker.Enumeration;

public class AppScannerService : IAppScannerService
{
    private readonly Win32AppEnumerator _win32Enumerator;
    private readonly UwpAppEnumerator _uwpEnumerator;
    private readonly IIconService _iconService;
    private readonly IAppRepository _repository;
    private readonly ILogger<AppScannerService> _logger;

    public AppScannerService(Win32AppEnumerator win32Enumerator, UwpAppEnumerator uwpEnumerator,
        IIconService iconService, IAppRepository repository, ILogger<AppScannerService> logger)
    {
        _win32Enumerator = win32Enumerator;
        _uwpEnumerator = uwpEnumerator;
        _iconService = iconService;
        _repository = repository;
        _logger = logger;
    }

    public async IAsyncEnumerable<InstalledApp> ScanAllAppsAsync()
    {
        var win32Task = Task.Run(() => _win32Enumerator.EnumerateApps().ToList());
        var uwpTask   = Task.Run(() => _uwpEnumerator.EnumerateApps().ToList());
        var win32List = await win32Task;
        var uwpList   = await uwpTask;
        _logger.LogDebug("Enumerated {Win32} Win32 + {Uwp} UWP apps", win32List.Count, uwpList.Count);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var app in win32List.Concat(uwpList))
        {
            if (!seen.Add(app.Id)) continue;
            app.IconPath = await _iconService.ResolveIconAsync(app);
            _ = _repository.UpsertAppAsync(app).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "Upsert failed for {AppId}", app.Id);
            }, TaskContinuationOptions.OnlyOnFaulted);
            yield return app;
        }
    }

    public Task<IEnumerable<InstalledApp>> GetCachedAppsAsync() =>
        _repository.GetAllAppsAsync();
}
