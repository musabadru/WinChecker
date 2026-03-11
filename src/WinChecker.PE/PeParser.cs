using AsmResolver.PE;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Win32Resources;
using WinChecker.Core;
using WinChecker.Core.Models;
using WinChecker.Core.Services;
using AsmResolver;

namespace WinChecker.PE;

public class PeParser : IPeParser
{
    private readonly IDllResolver _dllResolver;

    public PeParser(IDllResolver dllResolver)
    {
        _dllResolver = dllResolver;
    }

    public async Task<PeMetadata> ParseMetadataAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var metadata = new PeMetadata();
            var appDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

            try
            {
                var peFile = PEFile.FromFile(filePath);
                var peImage = PEImage.FromFile(peFile);
                
                // Architecture
                metadata.Architecture = peFile.FileHeader.Machine switch
                {
                    MachineType.I386 => WinChecker.Core.Architecture.X86,
                    MachineType.Amd64 => WinChecker.Core.Architecture.X64,
                    MachineType.Arm => WinChecker.Core.Architecture.Arm,
                    MachineType.Arm64 => WinChecker.Core.Architecture.Arm64,
                    _ => WinChecker.Core.Architecture.Unknown
                };

                // Subsystem
                metadata.Subsystem = peImage.SubSystem.ToString();

                // Linker Version
                metadata.LinkerVersion = new Version(peFile.OptionalHeader.MajorLinkerVersion, peFile.OptionalHeader.MinorLinkerVersion);

                // TimeDateStamp
                metadata.CompileTime = peImage.TimeDateStamp;

                // Dependencies (Imports)
                foreach (var module in peImage.Imports)
                {
                    var dllName = module.Name ?? "Unknown";
                    var resolvedPath = _dllResolver.ResolveDllPath(dllName, appDirectory, metadata.Architecture);
                    
                    metadata.Dependencies.Add(new DependencyInfo
                    {
                        Name = dllName,
                        ResolvedPath = resolvedPath,
                        IsMissing = resolvedPath == null,
                        IsApiSet = dllName?.StartsWith("api-ms-win-", StringComparison.OrdinalIgnoreCase) ?? false
                    });
                }

                // Exports
                if (peImage.Exports != null)
                {
                    foreach (var export in peImage.Exports.Entries)
                    {
                        if (export.Name != null)
                        {
                            metadata.Exports.Add(export.Name);
                        }
                    }
                }

                // Resources (Manifest)
                if (peImage.Resources != null)
                {
                    var manifest = ExtractManifest(peImage.Resources);
                    if (manifest != null)
                    {
                        metadata.Manifest = manifest;
                    }

                    metadata.VersionInfo = ExtractVersionInfo(peImage.Resources);
                }
            }
            catch (Exception)
            {
                // Handle or log error (e.g., file not found, not a PE file)
            }

            return metadata;
        });
    }

    private Dictionary<string, string> ExtractVersionInfo(IResourceDirectory resources)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Version Info is RT_VERSION (ID 16) -> Name 1 -> Language Neutral or other
        var versionType = resources.Entries.FirstOrDefault(e => e.Id == 16) as IResourceDirectory;
        if (versionType == null) return result;

        var versionName = versionType.Entries.FirstOrDefault() as IResourceDirectory;
        if (versionName == null) return result;

        var versionLang = versionName.Entries.FirstOrDefault() as IResourceData;
        if (versionLang == null || versionLang.Contents == null) return result;

        try
        {
            if (versionLang.Contents is IReadableSegment readable)
            {
                var data = readable.ToArray();
                // Simple parsing of VS_VERSIONINFO might be complex, so we'll use a basic heuristic or a helper
                // For now, let's just mark it as found. A full parser would be better.
                // TODO: Implement a robust VS_VERSIONINFO parser if needed, or use a library.
            }
        }
        catch { }

        return result;
    }

    private string? ExtractManifest(IResourceDirectory resources)
    {
        // RT_MANIFEST is type 24
        var manifestType = resources.Entries.FirstOrDefault(e => e.Id == 24) as IResourceDirectory;
        if (manifestType != null)
        {
            // Usually Name 1
            var manifestName = manifestType.Entries.FirstOrDefault() as IResourceDirectory;
            if (manifestName != null)
            {
                // Language neutral or 1033
                var manifestLang = manifestName.Entries.FirstOrDefault() as IResourceData;
                if (manifestLang != null && manifestLang.Contents is IReadableSegment readable)
                {
                    var data = readable.ToArray();
                    return System.Text.Encoding.UTF8.GetString(data);
                }
            }
        }
        return null;
    }
}
