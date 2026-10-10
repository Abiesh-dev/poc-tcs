using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserDirectory.Application.Abstractions;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DatabaseOptions _options;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        AppDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IOptions<DatabaseOptions> options,
        ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migrations applied");

        if (_options.SeedSampleData)
        {
            await SeedAsync(cancellationToken);
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = _dateTimeProvider.UtcNow;

        _dbContext.Users.AddRange(
            new User("Aarav Sharma", 32, "Chennai", "Tamil Nadu", "600001", now),
            new User("Meera Iyer", 27, "Bengaluru", "Karnataka", "560034", now),
            new User("Rohan Gupta", 45, "Pune", "Maharashtra", "411001", now));

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded sample users");
    }
}
