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

    public AppScannerService(Win32AppEnumerator win32Enumerator, UwpAppEnumerator uwpEnumerator,
        IIconService iconService, IAppRepository repository)
    {
        _win32Enumerator = win32Enumerator;
        _uwpEnumerator = uwpEnumerator;
        _iconService = iconService;
        _repository = repository;
    }

    public async IAsyncEnumerable<InstalledApp> ScanAllAppsAsync()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var app in _win32Enumerator.EnumerateApps().Concat(_uwpEnumerator.EnumerateApps()))
        {
            if (!seen.Add(app.Id)) continue;
            app.IconPath = await _iconService.ResolveIconAsync(app);
            await _repository.UpsertAppAsync(app);
            yield return app;
        }
    }

    public Task<IEnumerable<InstalledApp>> GetCachedAppsAsync() =>
        _repository.GetAllAppsAsync();
}
