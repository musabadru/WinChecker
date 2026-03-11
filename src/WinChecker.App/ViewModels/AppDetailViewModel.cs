using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using WinChecker.Core;
using WinChecker.Core.Models;
using WinChecker.Core.Services;

namespace WinChecker.App.ViewModels;

public record VersionEntry(string Key, string Value);

public partial class AppDetailViewModel : ObservableObject
{
    private readonly IPeParser _peParser;

    [ObservableProperty]
    private InstalledApp? _app;

    [ObservableProperty]
    private PeMetadata? _metadata;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<VersionEntry> VersionInfoItems { get; } = new();

    public AppDetailViewModel(IPeParser peParser)
    {
        _peParser = peParser;
    }

    public async Task InitializeAsync(InstalledApp app)
    {
        App = app;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // For Win32 apps, we usually look for the main executable
            // If InstallPath is a directory, we might need a better heuristic to find the main exe
            // For now, let's assume we can try to find an exe if InstallPath is valid
            string? targetFile = null;
            
            if (File.Exists(app.InstallPath))
            {
                targetFile = app.InstallPath;
            }
            else if (Directory.Exists(app.InstallPath))
            {
                var exes = Directory.GetFiles(app.InstallPath, "*.exe", SearchOption.TopDirectoryOnly);
                targetFile = exes.OrderBy(e => e.Length).FirstOrDefault();
            }

            if (targetFile != null)
            {
                Metadata = await _peParser.ParseMetadataAsync(targetFile);

                VersionInfoItems.Clear();
                if (Metadata.VersionInfo != null)
                    foreach (var kv in Metadata.VersionInfo)
                        VersionInfoItems.Add(new VersionEntry(kv.Key, kv.Value));

                if (Metadata.Architecture != Architecture.Unknown)
                    App.Architecture = Metadata.Architecture;
            }
            else
            {
                ErrorMessage = "Could not locate main executable for inspection.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to parse PE metadata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
