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

    private static readonly HashSet<string> KnownDlls = LoadKnownDlls();

    public string? ResolveDllPath(string dllName, string appDirectory, Architecture appArchitecture)
    {
        if (string.IsNullOrWhiteSpace(dllName)) return null;

        // 1. API Sets (api-ms-win-*) - mark as system resolved for now
        if (dllName.StartsWith("api-ms-win-", StringComparison.OrdinalIgnoreCase))
        {
            return "System Resolved (API Set)";
        }

        // 2. KnownDLLs check
        if (KnownDlls.Contains(dllName.ToLowerInvariant()))
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
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
                catch { /* Ignore invalid paths */ }
            }
        }

        return null;
    }

    private static HashSet<string> LoadKnownDlls()
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
        catch { /* Fallback to empty if registry access fails */ }
        return dlls;
    }
}
