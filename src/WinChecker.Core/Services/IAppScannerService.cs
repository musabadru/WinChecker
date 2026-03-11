using WinChecker.Core.Models;

namespace WinChecker.Core.Services;

public interface IAppScannerService
{
    Task<IEnumerable<InstalledApp>> ScanAllAppsAsync();
}
