using FluentAssertions;
using UserDirectory.Domain.Entities;
using UserDirectory.Domain.Exceptions;
using UserDirectory.Infrastructure.Persistence;
using Xunit;

namespace UserDirectory.UnitTests.Infrastructure;

public class UserRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteDatabaseFixture _fixture = new();

    [Fact]
    public async Task AddAsync_AssignsAGeneratedIdentifier()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);

        await repository.AddAsync(user, CancellationToken.None);

        user.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersOrderedByName()
    {
        await SeedAsync(
            new User("Zara Khan", 30, "Delhi", "Delhi", "110001", Now),
            new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now),
            new User("Meera Iyer", 27, "Chennai", "Tamil Nadu", "600002", Now));

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        var users = await repository.GetAllAsync(CancellationToken.None);

        users.Select(user => user.Name).Should().Equal("Ada Lovelace", "Meera Iyer", "Zara Khan");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollectionForAnEmptyTable()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        var users = await repository.GetAllAsync(CancellationToken.None);

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenMissing()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByIdAsync(12345, CancellationToken.None);

        user.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_TranslatesTheUniqueIndexViolationIntoADomainException()
    {
        await SeedAsync(new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now));

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);
        var duplicate = new User("Ada Lovelace", 40, "Bristol", "Bristol", "600001", Now);

        var act = () => repository.AddAsync(duplicate, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateUserException>();
    }

    [Fact]
    public async Task AddAsync_TreatsNamesAndPincodesCaseInsensitivelyForUniqueness()
    {
        await SeedAsync(new User("Ada Lovelace", 36, "London", "Greater London", "ab600001", Now));

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);
        var duplicate = new User("ADA LOVELACE", 40, "Bristol", "Bristol", "AB600001", Now);

        var act = () => repository.AddAsync(duplicate, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateUserException>();
    }

    [Fact]
    public async Task AddAsync_AllowsTheSameNameInADifferentPincode()
    {
        await SeedAsync(new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now));

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);
        var sibling = new User("Ada Lovelace", 40, "Bristol", "Bristol", "600002", Now);

        await repository.AddAsync(sibling, CancellationToken.None);

        sibling.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExistsAsync_MatchesIgnoringCase()
    {
        await SeedAsync(new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now));

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        var exists = await repository.ExistsAsync("ada lovelace", "600001", null, CancellationToken.None);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_IgnoresTheExcludedIdentifier()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        await SeedAsync(user);

        await using var context = _fixture.CreateContext();
        var repository = new UserRepository(context);

        var exists = await repository.ExistsAsync("Ada Lovelace", "600001", user.Id, CancellationToken.None);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheNewValues()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        await SeedAsync(user);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new UserRepository(context);
            var loaded = await repository.GetByIdAsync(user.Id, CancellationToken.None);
            loaded!.UpdateDetails("Grace Hopper", 45, "New York", "New York", "10001", Now.AddDays(1));
            await repository.UpdateAsync(loaded, CancellationToken.None);
        }

        await using var verification = _fixture.CreateContext();
        var reloaded = await new UserRepository(verification).GetByIdAsync(user.Id, CancellationToken.None);

        reloaded!.Name.Should().Be("Grace Hopper");
        reloaded.UpdatedAtUtc.Should().Be(Now.AddDays(1));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheRow()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        await SeedAsync(user);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new UserRepository(context);
            var loaded = await repository.GetByIdAsync(user.Id, CancellationToken.None);
            await repository.DeleteAsync(loaded!, CancellationToken.None);
        }

        await using var verification = _fixture.CreateContext();
        var reloaded = await new UserRepository(verification).GetByIdAsync(user.Id, CancellationToken.None);

        reloaded.Should().BeNull();
    }

    private async Task SeedAsync(params User[] users)
    {
        await using var context = _fixture.CreateContext();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    public void Dispose() => _fixture.Dispose();
}
