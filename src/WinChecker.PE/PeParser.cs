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
using Microsoft.Extensions.Logging;
using System.Text;

namespace WinChecker.PE;

public class PeParser : IPeParser
{
    private readonly IDllResolver _dllResolver;
    private readonly ILogger<PeParser> _logger;

    public PeParser(IDllResolver dllResolver, ILogger<PeParser> logger)
    {
        _dllResolver = dllResolver;
        _logger = logger;
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
                ParseVersionNode(data, 0, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse VS_VERSIONINFO resource");
        }

        return result;
    }

    private static string ReadWChar(byte[] data, ref int offset)
    {
        int start = offset;
        while (offset + 1 < data.Length && !(data[offset] == 0 && data[offset + 1] == 0))
            offset += 2;
        var value = Encoding.Unicode.GetString(data, start, offset - start);
        offset += 2; // consume null terminator
        if (offset % 4 != 0) offset += 4 - (offset % 4);
        return value;
    }

    private static void ParseVersionNode(byte[] data, int nodeStart, Dictionary<string, string> result)
    {
        if (nodeStart + 6 > data.Length) return;
        int offset = nodeStart;
        ushort wLength      = BitConverter.ToUInt16(data, offset); offset += 2;
        ushort wValueLength = BitConverter.ToUInt16(data, offset); offset += 2;
        ushort wType        = BitConverter.ToUInt16(data, offset); offset += 2;
        if (wLength == 0 || nodeStart + wLength > data.Length) return;

        string key = ReadWChar(data, ref offset);

        if (wValueLength > 0)
        {
            if (wType == 1) // text String leaf
            {
                int byteLen = wValueLength * 2;
                if (offset + byteLen <= data.Length)
                    result[key] = Encoding.Unicode.GetString(data, offset, byteLen).TrimEnd('\0');
            }
            int advance = wType == 1 ? wValueLength * 2 : wValueLength;
            offset += advance;
            if (offset % 4 != 0) offset += 4 - (offset % 4);
        }

        int nodeEnd = nodeStart + wLength;
        while (offset < nodeEnd - 6)
        {
            if (offset % 4 != 0) offset += 4 - (offset % 4);
            if (offset >= nodeEnd) break;
            ushort childLen = BitConverter.ToUInt16(data, offset);
            if (childLen == 0) break;
            ParseVersionNode(data, offset, result);
            offset += childLen;
            if (offset % 4 != 0) offset += 4 - (offset % 4);
        }
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
