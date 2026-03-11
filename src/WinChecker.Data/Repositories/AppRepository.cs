using Dapper;
using Microsoft.Data.Sqlite;
using WinChecker.Core;
using WinChecker.Core.Models;
using WinChecker.Core.Repositories;

namespace WinChecker.Data.Repositories;

public class AppRepository : IAppRepository
{
    private readonly string _connectionString;

    public AppRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<InstalledApp>> GetAllAppsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<InstalledApp>("SELECT * FROM apps");
    }

    public async Task<InstalledApp?> GetAppByIdAsync(string id)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<InstalledApp>(
            "SELECT * FROM apps WHERE id = @id", new { id });
    }

    public async Task UpsertAppAsync(InstalledApp app)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            INSERT INTO apps (id, name, version, publisher, architecture, install_date, install_path, source)
            VALUES (@Id, @Name, @Version, @Publisher, @Architecture, @InstallDate, @InstallPath, @Source)
            ON CONFLICT(id) DO UPDATE SET
                name = EXCLUDED.name,
                version = EXCLUDED.version,
                publisher = EXCLUDED.publisher,
                architecture = EXCLUDED.architecture,
                install_date = EXCLUDED.install_date,
                install_path = EXCLUDED.install_path,
                source = EXCLUDED.source;";
        
        await connection.ExecuteAsync(sql, app);
    }

    public async Task DeleteAppAsync(string id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM apps WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<InstalledApp>> SearchAppsAsync(string query)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            SELECT a.* FROM apps a
            JOIN apps_fts f ON a.rowid = f.rowid
            WHERE apps_fts MATCH @query
            ORDER BY rank;";
            
        return await connection.QueryAsync<InstalledApp>(sql, new { query });
    }
}
