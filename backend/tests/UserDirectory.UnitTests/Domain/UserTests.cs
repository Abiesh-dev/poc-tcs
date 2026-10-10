using FluentAssertions;
using UserDirectory.Domain.Entities;
using UserDirectory.Domain.Exceptions;
using Xunit;

namespace UserDirectory.UnitTests.Domain;

public class UserTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_TrimsTextFields()
    {
        var user = new User("  Ada Lovelace  ", 36, " London ", " Greater London ", " 600001 ", Now);

        user.Name.Should().Be("Ada Lovelace");
        user.City.Should().Be("London");
        user.State.Should().Be("Greater London");
        user.Pincode.Should().Be("600001");
    }

    [Fact]
    public void Constructor_SetsCreatedAndUpdatedTimestamps()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);

        user.CreatedAtUtc.Should().Be(Now);
        user.UpdatedAtUtc.Should().Be(Now);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Constructor_RejectsInvalidName(string name)
    {
        var act = () => new User(name, 36, "London", "Greater London", "600001", Now);

        act.Should().Throw<InvalidUserDataException>()
            .Which.Field.Should().Be(nameof(User.Name));
    }

    [Fact]
    public void Constructor_RejectsNameLongerThanMaximum()
    {
        var name = new string('a', UserConstraints.NameMaxLength + 1);

        var act = () => new User(name, 36, "London", "Greater London", "600001", Now);

        act.Should().Throw<InvalidUserDataException>()
            .Which.Field.Should().Be(nameof(User.Name));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Constructor_RejectsAgeOutsideAllowedRange(int age)
    {
        var act = () => new User("Ada Lovelace", age, "London", "Greater London", "600001", Now);

        act.Should().Throw<InvalidUserDataException>()
            .Which.Field.Should().Be(nameof(User.Age));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void Constructor_AcceptsAgeBoundaries(int age)
    {
        var user = new User("Ada Lovelace", age, "London", "Greater London", "600001", Now);

        user.Age.Should().Be(age);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    public void Constructor_RejectsPincodeOutsideLengthRange(string pincode)
    {
        var act = () => new User("Ada Lovelace", 36, "London", "Greater London", pincode, Now);

        act.Should().Throw<InvalidUserDataException>()
            .Which.Field.Should().Be(nameof(User.Pincode));
    }

    [Fact]
    public void UpdateDetails_ReplacesValuesAndAdvancesUpdatedTimestamp()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);
        var later = Now.AddHours(5);

        user.UpdateDetails("Grace Hopper", 45, "New York", "New York", "10001", later);

        user.Name.Should().Be("Grace Hopper");
        user.Age.Should().Be(45);
        user.City.Should().Be("New York");
        user.State.Should().Be("New York");
        user.Pincode.Should().Be("10001");
        user.CreatedAtUtc.Should().Be(Now);
        user.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void UpdateDetails_LeavesEntityUnchangedWhenValidationFails()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "600001", Now);

        var act = () => user.UpdateDetails("Grace Hopper", 500, "New York", "New York", "10001", Now.AddHours(5));

        act.Should().Throw<InvalidUserDataException>();
        user.Name.Should().Be("Ada Lovelace");
        user.Age.Should().Be(36);
        user.City.Should().Be("London");
        user.UpdatedAtUtc.Should().Be(Now);
    }
}
