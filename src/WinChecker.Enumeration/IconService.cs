using Microsoft.Win32;
using System.Drawing;
using System.IO;
using WinChecker.Core;
using WinChecker.Core.Services;

namespace WinChecker.Enumeration;

public class IconService : IIconService
{
    private readonly string _iconCachePath;

    public IconService()
    {
        _iconCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinChecker", "IconCache");
        Directory.CreateDirectory(_iconCachePath);
    }

    public async Task<string?> ResolveIconAsync(InstalledApp app)
    {
        return await Task.Run(() =>
        {
            var cachedIconPath = Path.Combine(_iconCachePath, $"{app.Id}.png");
            if (File.Exists(cachedIconPath)) return cachedIconPath;

            string? sourcePath = null;

            if (app.Source == AppSource.Win32)
            {
                sourcePath = ResolveWin32IconPath(app);
            }
            else if (app.Source == AppSource.Uwp)
            {
                // UWP icons are handled differently, usually assets in the install folder
                // For now, return null and we can refine later
                return null;
            }

            if (sourcePath != null && File.Exists(sourcePath))
            {
                try
                {
                    using var icon = Icon.ExtractAssociatedIcon(sourcePath);
                    if (icon != null)
                    {
                        using var bitmap = icon.ToBitmap();
                        bitmap.Save(cachedIconPath, System.Drawing.Imaging.ImageFormat.Png);
                        return cachedIconPath;
                    }
                }
                catch { /* Ignore extraction errors */ }
            }

            // Strategy 2: Known Names
            if (!string.IsNullOrEmpty(app.InstallPath) && Directory.Exists(app.InstallPath))
            {
                var knownNames = new[] { "DisplayIcon.ico", "Icon.ico", "app.ico", "appicon.ico", "application.ico", "logo.ico" };
                foreach (var name in knownNames)
                {
                    var path = Path.Combine(app.InstallPath, name);
                    if (File.Exists(path))
                    {
                        File.Copy(path, cachedIconPath, true);
                        return cachedIconPath;
                    }
                }
            }

            return null;
        });
    }

    private string? ResolveWin32IconPath(InstalledApp app)
    {
        // 1. BasicIconGetter: Registry DisplayIcon
        var path = GetRegistryDisplayIcon(app.Id);
        if (!string.IsNullOrEmpty(path))
        {
            // Handle comma-separated index (e.g., "C:\path\app.exe,0")
            if (path.Contains(","))
            {
                path = path.Split(',')[0].Trim('"', ' ');
            }
            if (File.Exists(path)) return path;
        }

        // 3. FileIconGetter: Extract from main executable
        if (!string.IsNullOrEmpty(app.InstallPath) && Directory.Exists(app.InstallPath))
        {
            var exes = Directory.GetFiles(app.InstallPath, "*.exe", SearchOption.TopDirectoryOnly);
            // Heuristic: shortest name or name matching the app name
            var bestExe = exes.OrderBy(e => e.Length).FirstOrDefault();
            if (bestExe != null) return bestExe;
        }

        return null;
    }

    private string? GetRegistryDisplayIcon(string appId)
    {
        string[] keys = 
        {
            $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{appId}",
            $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{appId}"
        };

        foreach (var keyPath in keys)
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null)
            {
                var icon = key.GetValue("DisplayIcon") as string;
                if (!string.IsNullOrEmpty(icon)) return icon;
            }
        }

        using var userKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{appId}");
        return userKey?.GetValue("DisplayIcon") as string;
    }
}
