namespace WinChecker.Core.Models;

public class DependencyInfo
{
    public required string Name { get; set; }
    public string? ResolvedPath { get; set; }
    public bool IsMissing { get; set; }
    public bool IsApiSet { get; set; }
}

public class PeMetadata
{
    public Architecture Architecture { get; set; }
    public string? Subsystem { get; set; }
    public Version? LinkerVersion { get; set; }
    public DateTime? CompileTime { get; set; }
    public List<DependencyInfo> Dependencies { get; set; } = new();
    public List<string> Exports { get; set; } = new();
    public string? Manifest { get; set; }
    public Dictionary<string, string> VersionInfo { get; set; } = new();
}
