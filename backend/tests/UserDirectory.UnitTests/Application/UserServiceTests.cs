using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UserDirectory.Application.Abstractions;
using UserDirectory.Application.Users;
using UserDirectory.Application.Users.Dtos;
using UserDirectory.Domain.Entities;
using UserDirectory.Domain.Exceptions;
using UserDirectory.UnitTests.TestDoubles;
using Xunit;

namespace UserDirectory.UnitTests.Application;

public class UserServiceTests
{
    private static readonly DateTime Now = new(2026, 3, 1, 8, 30, 0, DateTimeKind.Utc);

    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly FixedDateTimeProvider _dateTimeProvider = new(Now);
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repository, _dateTimeProvider, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_MapsEveryUserToResponse()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>
        {
            new("Ada Lovelace", 36, "London", "Greater London", "600001", Now),
            new("Grace Hopper", 45, "New York", "New York", "10001", Now),
        });

        var result = await _sut.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(user => user.Name).Should().Equal("Ada Lovelace", "Grace Hopper");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyListWhenNoUsersExist()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>());

        var result = await _sut.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenUserDoesNotExist()
    {
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _sut.GetByIdAsync(42, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>()
            .Where(exception => exception.UserId == 42);
    }

    [Fact]
    public async Task CreateAsync_PersistsTrimmedUserAndReturnsResponse()
    {
        _repository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new CreateUserRequest("  Ada Lovelace ", 36, " London ", " Greater London ", " 600001 ");

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        result.Name.Should().Be("Ada Lovelace");
        result.City.Should().Be("London");
        result.Pincode.Should().Be("600001");
        result.CreatedAtUtc.Should().Be(Now);

        await _repository.Received(1).AddAsync(
            Arg.Is<User>(user => user.Name == "Ada Lovelace" && user.Pincode == "600001"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenNameAndPincodeAlreadyExist()
    {
        _repository
            .ExistsAsync("Ada Lovelace", "600001", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new CreateUserRequest("Ada Lovelace", 36, "London", "Greater London", "600001");

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateUserException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_SurfacesDuplicateDetectedByTheDatabase()
    {
        _repository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _repository
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateUserException("Ada Lovelace", "600001"));

        var request = new CreateUserRequest("Ada Lovelace", 36, "London", "Greater London", "600001");

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateUserException>();
    }

    [Fact]
    public async Task CreateAsync_RejectsValuesThatBreakDomainInvariants()
    {
        _repository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new CreateUserRequest("Ada Lovelace", 500, "London", "Greater London", "600001");

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidUserDataException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_AppliesChangesAndStampsUpdatedTime()
    {
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        _repository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _dateTimeProvider.UtcNow = Now.AddDays(1);

        var request = new UpdateUserRequest("Grace Hopper", 45, "New York", "New York", "10001");

        var result = await _sut.UpdateAsync(1, request, CancellationToken.None);

        result.Name.Should().Be("Grace Hopper");
        result.UpdatedAtUtc.Should().Be(Now.AddDays(1));
        result.CreatedAtUtc.Should().Be(Now);
        await _repository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ExcludesTheEditedUserFromTheDuplicateCheck()
    {
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        _repository.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(existing);
        _repository
            .ExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new UpdateUserRequest("Ada Lovelace", 37, "London", "Greater London", "600001");

        await _sut.UpdateAsync(7, request, CancellationToken.None);

        await _repository.Received(1).ExistsAsync("Ada Lovelace", "600001", 7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenUserDoesNotExist()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((User?)null);

        var request = new UpdateUserRequest("Grace Hopper", 45, "New York", "New York", "10001");

        var act = () => _sut.UpdateAsync(99, request, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenAnotherUserAlreadyUsesNameAndPincode()
    {
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        _repository
            .ExistsAsync("Grace Hopper", "10001", 1, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateUserRequest("Grace Hopper", 45, "New York", "New York", "10001");

        var act = () => _sut.UpdateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateUserException>();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_RemovesAnExistingUser()
    {
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);

        await _sut.DeleteAsync(1, CancellationToken.None);

        await _repository.Received(1).DeleteAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenUserDoesNotExist()
    {
        _repository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _sut.DeleteAsync(5, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
