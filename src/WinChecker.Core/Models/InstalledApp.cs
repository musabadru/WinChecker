namespace WinChecker.Core;

public enum AppSource
{
    Win32,
    Uwp,
    Portable
}

public enum Architecture
{
    Unknown,
    X86,
    X64,
    Arm,
    Arm64
}

public class InstalledApp
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? Publisher { get; set; }
    public Architecture Architecture { get; set; }
    public string? InstallDate { get; set; }
    public string? InstallPath { get; set; }
    public AppSource Source { get; set; }
    public string? IconPath { get; set; }
}
