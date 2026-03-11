using WinChecker.Core;

namespace WinChecker.Core.Services;

public interface IAppScannerService
{
    Task<IEnumerable<InstalledApp>> ScanAllAppsAsync();
}
