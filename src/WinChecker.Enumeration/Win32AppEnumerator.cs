using Microsoft.Win32;
using WinChecker.Core;

namespace WinChecker.Enumeration;

public class Win32AppEnumerator
{
    private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    public IEnumerable<InstalledApp> EnumerateApps()
    {
        var apps = new List<InstalledApp>();

        // Current User
        apps.AddRange(EnumerateRegistryKey(Registry.CurrentUser, UninstallKeyPath));

        // Local Machine 64-bit (or 32-bit if running on 32-bit OS)
        apps.AddRange(EnumerateRegistryKey(Registry.LocalMachine, UninstallKeyPath));

        // Local Machine 32-bit (on 64-bit OS)
        apps.AddRange(EnumerateRegistryKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"));

        return apps.GroupBy(a => a.Id).Select(g => g.First());
    }

    private IEnumerable<InstalledApp> EnumerateRegistryKey(RegistryKey rootKey, string subKeyPath)
    {
        using var key = rootKey.OpenSubKey(subKeyPath);
        if (key == null) yield break;

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var subKey = key.OpenSubKey(subKeyName);
            if (subKey == null) continue;

            var displayName = subKey.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(displayName)) continue;

            yield return new InstalledApp
            {
                Id = subKeyName,
                Name = displayName,
                Version = subKey.GetValue("DisplayVersion") as string,
                Publisher = subKey.GetValue("Publisher") as string,
                InstallDate = subKey.GetValue("InstallDate") as string,
                InstallPath = subKey.GetValue("InstallLocation") as string,
                Source = AppSource.Win32,
                Architecture = Architecture.Unknown // Will be refined later
            };
        }
    }
}
