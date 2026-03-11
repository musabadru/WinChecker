using WinChecker.Core;
using WinChecker.PE;

namespace WinChecker.PE.Tests;

public class DllResolverTests
{
    private readonly DllResolver _resolver = new();

    [Test]
    public async Task ResolveDllPath_Should_Handle_ApiSet()
    {
        var result = _resolver.ResolveDllPath("api-ms-win-core-processthreads-l1-1-0.dll", "C:	emp", Architecture.X64);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("API Set");
    }

    [Test]
    public async Task ResolveDllPath_Should_Find_KnownDlls()
    {
        // kernel32 is always a KnownDLL
        var result = _resolver.ResolveDllPath("kernel32.dll", "C:	emp", Architecture.X64);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("System32", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    public async Task ResolveDllPath_Should_Handle_X86_Redirection()
    {
        // On a 64-bit OS, x86 kernel32 should resolve to SysWOW64
        var result = _resolver.ResolveDllPath("kernel32.dll", "C:	emp", Architecture.X86);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("SysWOW64", StringComparison.OrdinalIgnoreCase);
    }
}
