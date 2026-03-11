using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace WinChecker.Data;

public class DatabaseMigrator
{
    private readonly string _connectionString;

    public DatabaseMigrator(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Migrate()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        var currentVersion = (long)(command.ExecuteScalar() ?? 0L);

        var provider = new EmbeddedFileProvider(typeof(DatabaseMigrator).Assembly, "WinChecker.Data.Migrations");
        var contents = provider.GetDirectoryContents("");

        var migrationFiles = contents
            .Where(f => f.Name.EndsWith(".sql"))
            .OrderBy(f => f.Name)
            .ToList();

        foreach (var file in migrationFiles)
        {
            var versionString = file.Name.Split('_')[0];
            if (int.TryParse(versionString, out int fileVersion))
            {
                if (fileVersion > currentVersion)
                {
                    using var stream = file.CreateReadStream();
                    using var reader = new StreamReader(stream);
                    var sql = reader.ReadToEnd();

                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        using var migCommand = connection.CreateCommand();
                        migCommand.Transaction = transaction;
                        migCommand.CommandText = sql;
                        migCommand.ExecuteNonQuery();

                        using var versionCommand = connection.CreateCommand();
                        versionCommand.Transaction = transaction;
                        versionCommand.CommandText = $"PRAGMA user_version = {fileVersion};";
                        versionCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
