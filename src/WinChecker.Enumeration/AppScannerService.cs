using WinChecker.Core.Models;
using WinChecker.Core.Services;

namespace WinChecker.Enumeration;

public class AppScannerService : IAppScannerService
{
    private readonly Win32AppEnumerator _win32Enumerator;
    private readonly UwpAppEnumerator _uwpEnumerator;

    public AppScannerService(Win32AppEnumerator win32Enumerator, UwpAppEnumerator uwpEnumerator)
    {
        _win32Enumerator = win32Enumerator;
        _uwpEnumerator = uwpEnumerator;
    }

    public async Task<IEnumerable<InstalledApp>> ScanAllAppsAsync()
    {
        return await Task.Run(() =>
        {
            var win32Apps = _win32Enumerator.EnumerateApps();
            var uwpApps = _uwpEnumerator.EnumerateApps();

            var allApps = win32Apps.Concat(uwpApps);

            // Deduplicate by ID
            return allApps.GroupBy(a => a.Id).Select(g => g.First()).ToList();
        });
    }
}
