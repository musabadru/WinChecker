using WinChecker.Core;
using WinChecker.Core.Services;

namespace WinChecker.Enumeration;

public class AppScannerService : IAppScannerService
{
    private readonly Win32AppEnumerator _win32Enumerator;
    private readonly UwpAppEnumerator _uwpEnumerator;
    private readonly IIconService _iconService;

    public AppScannerService(Win32AppEnumerator win32Enumerator, UwpAppEnumerator uwpEnumerator, IIconService iconService)
    {
        _win32Enumerator = win32Enumerator;
        _uwpEnumerator = uwpEnumerator;
        _iconService = iconService;
    }

    public async Task<IEnumerable<InstalledApp>> ScanAllAppsAsync()
    {
        var win32Apps = _win32Enumerator.EnumerateApps().ToList();
        var uwpApps = _uwpEnumerator.EnumerateApps().ToList();

        var allApps = win32Apps.Concat(uwpApps)
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .ToList();

        foreach (var app in allApps)
        {
            app.IconPath = await _iconService.ResolveIconAsync(app);
        }

        return allApps;
    }
}
