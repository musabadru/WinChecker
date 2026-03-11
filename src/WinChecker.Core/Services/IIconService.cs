namespace WinChecker.Core.Services;

public interface IIconService
{
    Task<string?> ResolveIconAsync(InstalledApp app);
}
