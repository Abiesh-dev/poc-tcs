using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserDirectory.Infrastructure.Persistence;

namespace UserDirectory.UnitTests.Infrastructure;

public sealed class SqliteDatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDatabaseFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.Migrate();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
