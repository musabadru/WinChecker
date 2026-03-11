using NSubstitute;
using WinChecker.Core;
using WinChecker.Core.Repositories;
using WinChecker.Core.Services;
using WinChecker.Enumeration;

namespace WinChecker.Enumeration.Tests;

public class AppScannerServiceTests
{
    private IIconService _iconService = null!;
    private IAppRepository _repository = null!;
    private AppScannerService _service = null!;

    [Before(Test)]
    public void SetUp()
    {
        _iconService = Substitute.For<IIconService>();
        _repository = Substitute.For<IAppRepository>();

        _iconService.ResolveIconAsync(Arg.Any<InstalledApp>())
            .Returns(Task.FromResult<string?>(null));
        _repository.UpsertAppAsync(Arg.Any<InstalledApp>())
            .Returns(Task.CompletedTask);
        _repository.GetAllAppsAsync()
            .Returns(Task.FromResult(Enumerable.Empty<InstalledApp>()));

        _service = new AppScannerService(
            new Win32AppEnumerator(),
            new UwpAppEnumerator(),
            _iconService,
            _repository);
    }

    [Test]
    public async Task ScanAllAppsAsync_YieldsAtLeastOneApp()
    {
        var apps = new List<InstalledApp>();
        await foreach (var app in _service.ScanAllAppsAsync())
            apps.Add(app);

        await Assert.That(apps.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ScanAllAppsAsync_DeduplicatesApps()
    {
        var apps = new List<InstalledApp>();
        await foreach (var app in _service.ScanAllAppsAsync())
            apps.Add(app);

        var uniqueCount = apps.Select(a => a.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        await Assert.That(apps.Count).IsEqualTo(uniqueCount);
    }

    [Test]
    public async Task ScanAllAppsAsync_CallsUpsertForEachApp()
    {
        var count = 0;
        await foreach (var app in _service.ScanAllAppsAsync())
            count++;

        await _repository.Received(count).UpsertAppAsync(Arg.Any<InstalledApp>());
    }

    [Test]
    public async Task GetCachedAppsAsync_DelegatesToRepository()
    {
        var expected = new List<InstalledApp>
        {
            new InstalledApp { Id = "cached1", Name = "Cached App" }
        };
        _repository.GetAllAppsAsync()
            .Returns(Task.FromResult<IEnumerable<InstalledApp>>(expected));

        var result = (await _service.GetCachedAppsAsync()).ToList();

        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result[0].Id).IsEqualTo("cached1");
    }
}
