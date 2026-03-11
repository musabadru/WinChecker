using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using WinChecker.Core;
using WinChecker.Core.Services;

namespace WinChecker.PE;

public class DllResolver : IDllResolver
{
    private static readonly string[] SystemPaths =
    {
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
    };

    private readonly ILogger<DllResolver> _logger;
    private readonly HashSet<string> _knownDlls;

    public DllResolver(ILogger<DllResolver> logger)
    {
        _logger = logger;
        _knownDlls = LoadKnownDlls();
    }

    public string? ResolveDllPath(string dllName, string appDirectory, Architecture appArchitecture)
    {
        if (string.IsNullOrWhiteSpace(dllName)) return null;

        // 1. API Sets (api-ms-win-*) - mark as system resolved for now
        if (dllName.StartsWith("api-ms-win-", StringComparison.OrdinalIgnoreCase))
        {
            return "System Resolved (API Set)";
        }

        // 2. KnownDLLs check
        if (_knownDlls.Contains(dllName.ToLowerInvariant()))
        {
            var systemPath = appArchitecture == Architecture.X86
                ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
                : Environment.GetFolderPath(Environment.SpecialFolder.System);

            var path = Path.Combine(systemPath, dllName);
            if (File.Exists(path)) return path;
        }

        // 3. Application Directory
        var appPath = Path.Combine(appDirectory, dllName);
        if (File.Exists(appPath)) return appPath;

        // 4. System Folders
        // If app is x86 and we are on x64, check SysWOW64 first
        var sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var sysWow64 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);

        if (appArchitecture == Architecture.X86)
        {
            var path = Path.Combine(sysWow64, dllName);
            if (File.Exists(path)) return path;

            path = Path.Combine(sys32, dllName); // Fallback
            if (File.Exists(path)) return path;
        }
        else
        {
            var path = Path.Combine(sys32, dllName);
            if (File.Exists(path)) return path;
        }

        // 5. Windows Directory
        var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var winPath = Path.Combine(winDir, dllName);
        if (File.Exists(winPath)) return winPath;

        // 6. PATH environment variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (var p in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var fullPath = Path.Combine(p, dllName);
                    if (File.Exists(fullPath)) return fullPath;
                }
                catch (Exception ex) { _logger.LogDebug(ex, "Skipping invalid PATH entry: {Entry}", p); }
            }
        }

        return null;
    }

    private HashSet<string> LoadKnownDlls()
    {
        var dlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\KnownDLLs");
            if (key != null)
            {
                foreach (var name in key.GetValueNames())
                {
                    var value = key.GetValue(name) as string;
                    if (value != null)
                    {
                        dlls.Add(value);
                    }
                }
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to read KnownDLLs from registry"); }
        return dlls;
    }
}
