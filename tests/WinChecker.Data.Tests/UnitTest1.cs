using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WinChecker.Core;
using WinChecker.Core.Models;
using WinChecker.Core.Repositories;
using WinChecker.Data;
using WinChecker.Data.Repositories;

namespace WinChecker.Data.Tests;

public class AppRepositoryTests
{
    private const string ConnStr = "Data Source=testdb;Mode=Memory;Cache=Shared";
    private SqliteConnection _keepAlive = null!;
    private IAppRepository _repo = null!;

    [Before(Test)]
    public void SetUp()
    {
        _keepAlive = new SqliteConnection(ConnStr);
        _keepAlive.Open();

        var opts = Options.Create(new DatabaseOptions { ConnectionString = ConnStr });
        new DatabaseMigrator(opts).Migrate();
        _repo = new AppRepository(opts);
    }

    [After(Test)]
    public void TearDown()
    {
        _keepAlive.Dispose();
    }

    [Test]
    public async Task UpsertAppAsync_InsertsNewApp()
    {
        var app = new InstalledApp { Id = "app1", Name = "My App", Publisher = "Publisher A" };

        await _repo.UpsertAppAsync(app);

        var result = await _repo.GetAppByIdAsync("app1");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Name).IsEqualTo("My App");
    }

    [Test]
    public async Task UpsertAppAsync_UpdatesExistingApp()
    {
        var app = new InstalledApp { Id = "app2", Name = "Old Name" };
        await _repo.UpsertAppAsync(app);

        app.Name = "New Name";
        await _repo.UpsertAppAsync(app);

        var all = (await _repo.GetAllAppsAsync()).Where(a => a.Id == "app2").ToList();
        await Assert.That(all.Count).IsEqualTo(1);
        await Assert.That(all[0].Name).IsEqualTo("New Name");
    }

    [Test]
    public async Task GetAllAppsAsync_ReturnsAllInserted()
    {
        var app1 = new InstalledApp { Id = "bulk1", Name = "App One" };
        var app2 = new InstalledApp { Id = "bulk2", Name = "App Two" };
        await _repo.UpsertAppAsync(app1);
        await _repo.UpsertAppAsync(app2);

        var all = (await _repo.GetAllAppsAsync()).ToList();

        await Assert.That(all.Count(a => a.Id == "bulk1" || a.Id == "bulk2")).IsEqualTo(2);
    }

    [Test]
    public async Task SearchAppsAsync_FindsByName()
    {
        var app = new InstalledApp { Id = "fts1", Name = "WinChecker Studio" };
        await _repo.UpsertAppAsync(app);

        var results = (await _repo.SearchAppsAsync("WinChecker")).ToList();

        await Assert.That(results.Any(r => r.Id == "fts1")).IsTrue();
    }

    [Test]
    public async Task SearchAppsAsync_FindsByPublisher()
    {
        var app = new InstalledApp { Id = "fts2", Name = "Some App", Publisher = "Acme Corporation" };
        await _repo.UpsertAppAsync(app);

        var results = (await _repo.SearchAppsAsync("Acme")).ToList();

        await Assert.That(results.Any(r => r.Id == "fts2")).IsTrue();
    }

    [Test]
    public async Task DeleteAppAsync_RemovesApp()
    {
        var app = new InstalledApp { Id = "del1", Name = "To Delete" };
        await _repo.UpsertAppAsync(app);

        await _repo.DeleteAppAsync("del1");

        var result = await _repo.GetAppByIdAsync("del1");
        await Assert.That(result).IsNull();
    }
}
