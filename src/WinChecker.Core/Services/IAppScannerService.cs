using WinChecker.Core;

namespace WinChecker.Core.Services;

public interface IAppScannerService
{
    IAsyncEnumerable<InstalledApp> ScanAllAppsAsync();
    Task<IEnumerable<InstalledApp>> GetCachedAppsAsync();
}
