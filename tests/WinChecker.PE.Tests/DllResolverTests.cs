using Microsoft.Extensions.Logging;
using NSubstitute;
using WinChecker.Core;
using WinChecker.PE;
using System.IO;

namespace WinChecker.PE.Tests;

public class DllResolverTests
{
    private readonly DllResolver _resolver = new(Substitute.For<ILogger<DllResolver>>());

    [Test]
    public async Task ResolveDllPath_Should_Handle_ApiSet()
    {
        var result = _resolver.ResolveDllPath("api-ms-win-core-processthreads-l1-1-0.dll", @"C:\temp", Architecture.X64);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("API Set");
    }

    [Test]
    public async Task ResolveDllPath_Should_Find_KnownDlls()
    {
        // kernel32 is always a KnownDLL
        var result = _resolver.ResolveDllPath("kernel32.dll", @"C:\temp", Architecture.X64);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("System32", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    public async Task ResolveDllPath_Should_Handle_X86_Redirection()
    {
        // On a 64-bit OS, x86 kernel32 should resolve to SysWOW64
        var result = _resolver.ResolveDllPath("kernel32.dll", @"C:\temp", Architecture.X86);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("SysWOW64", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    public async Task ResolveDllPath_Should_Check_AppDirectory()
    {
        // Create a dummy DLL in a temp dir
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dllPath = Path.Combine(tempDir, "test_dummy.dll");
        File.WriteAllText(dllPath, "dummy");

        try
        {
            var result = _resolver.ResolveDllPath("test_dummy.dll", tempDir, Architecture.X64);
            await Assert.That(result).IsEqualTo(dllPath);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task ResolveDllPath_Should_Return_Null_For_NonExistent()
    {
        var result = _resolver.ResolveDllPath("non_existent_dll_123.dll", @"C:\temp", Architecture.X64);
        await Assert.That(result).IsNull();
    }
}
