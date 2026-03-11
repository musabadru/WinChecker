using Microsoft.Extensions.Logging;
using NSubstitute;
using WinChecker.Core;
using WinChecker.Core.Services;
using WinChecker.PE;

namespace WinChecker.PE.Tests;

public class PeParserTests
{
    private static PeParser CreateParser()
    {
        var dllResolver = Substitute.For<IDllResolver>();
        var logger = Substitute.For<ILogger<PeParser>>();
        return new PeParser(dllResolver, logger);
    }

    [Test]
    public async Task ExtractVersionInfo_ReturnsKnownFields_ForKernel32()
    {
        var parser = CreateParser();
        var kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

        var metadata = await parser.ParseMetadataAsync(kernel32);

        await Assert.That(metadata.VersionInfo.ContainsKey("FileVersion")).IsTrue();
        await Assert.That(metadata.VersionInfo["FileVersion"]).IsNotEmpty();
        await Assert.That(metadata.VersionInfo.ContainsKey("ProductName")).IsTrue();
        await Assert.That(metadata.VersionInfo["ProductName"]).IsNotEmpty();
    }

    [Test]
    public async Task ParseMetadataAsync_ExtractsArchitecture_ForKernel32()
    {
        var parser = CreateParser();
        var kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

        var metadata = await parser.ParseMetadataAsync(kernel32);

        await Assert.That(metadata.Architecture).IsEqualTo(Architecture.X64);
    }

    [Test]
    public async Task ParseMetadataAsync_ReturnsDependencies_ForKernel32()
    {
        var parser = CreateParser();
        var kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

        var metadata = await parser.ParseMetadataAsync(kernel32);

        await Assert.That(metadata.Dependencies.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ParseMetadataAsync_ReturnsDefault_ForNonPeFile()
    {
        var parser = CreateParser();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a PE file");

            var metadata = await parser.ParseMetadataAsync(tempFile);

            await Assert.That(metadata.Architecture).IsEqualTo(Architecture.Unknown);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Test]
    public async Task ParseMetadataAsync_ExtractsManifest_ForNotepad()
    {
        var parser = CreateParser();
        var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var notepad = File.Exists(Path.Combine(system32, "notepad.exe"))
            ? Path.Combine(system32, "notepad.exe")
            : Path.Combine(windows, "notepad.exe");

        var metadata = await parser.ParseMetadataAsync(notepad);

        await Assert.That(metadata.Manifest).IsNotNull();
        await Assert.That(metadata.Manifest!.Contains("assembly")).IsTrue();
    }
}
