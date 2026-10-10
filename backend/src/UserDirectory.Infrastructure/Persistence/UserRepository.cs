using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserDirectory.Application.Abstractions;
using UserDirectory.Domain.Entities;
using UserDirectory.Domain.Exceptions;

namespace UserDirectory.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private const int SqliteConstraintErrorCode = 19;

    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(string name, string pincode, int? excludeId, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Name == name
                        && user.Pincode == pincode
                        && (excludeId == null || user.Id != excludeId),
                cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await SaveChangesAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);
        return SaveChangesAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            _dbContext.Entry(user).State = EntityState.Detached;
            throw new DuplicateUserException(user.Name, user.Pincode);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintErrorCode } sqliteException
        && sqliteException.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
}
