using Windows.Management.Deployment;
using WinChecker.Core.Models;

namespace WinChecker.Enumeration;

public class UwpAppEnumerator
{
    private readonly PackageManager _packageManager = new();

    public IEnumerable<InstalledApp> EnumerateApps()
    {
        var packages = _packageManager.FindPackagesForUser(string.Empty);
        foreach (var package in packages)
        {
            var id = package.Id.FullName;
            var name = package.DisplayName;
            
            // Some system packages might not have a display name
            if (string.IsNullOrWhiteSpace(name))
            {
                name = package.Id.Name;
            }

            var version = package.Id.Version;
            var versionString = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            Architecture arch = package.Id.Architecture switch
            {
                Windows.System.ProcessorArchitecture.X86 => Architecture.X86,
                Windows.System.ProcessorArchitecture.X64 => Architecture.X64,
                Windows.System.ProcessorArchitecture.Arm => Architecture.Arm,
                Windows.System.ProcessorArchitecture.Arm64 => Architecture.Arm64,
                _ => Architecture.Unknown
            };

            yield return new InstalledApp
            {
                Id = id,
                Name = name,
                Version = versionString,
                Publisher = package.PublisherDisplayName,
                Architecture = arch,
                InstallPath = package.InstalledLocation?.Path,
                Source = AppSource.Uwp,
                // Logo/Icon resolution can be added later
            };
        }
    }
}
