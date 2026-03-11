using WinChecker.Core;

namespace WinChecker.Core.Repositories;

public interface IAppRepository
{
    Task<IEnumerable<InstalledApp>> GetAllAppsAsync();
    Task<InstalledApp?> GetAppByIdAsync(string id);
    Task UpsertAppAsync(InstalledApp app);
    Task DeleteAppAsync(string id);
    Task<IEnumerable<InstalledApp>> SearchAppsAsync(string query);
}
