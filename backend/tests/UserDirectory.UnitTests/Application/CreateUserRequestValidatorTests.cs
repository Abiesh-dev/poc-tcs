using FluentAssertions;
using UserDirectory.Application.Users.Dtos;
using UserDirectory.Application.Users.Validation;
using Xunit;

namespace UserDirectory.UnitTests.Application;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _sut = new();

    private static CreateUserRequest ValidRequest() =>
        new("Ada Lovelace", 36, "London", "Greater London", "600001");

    [Fact]
    public void Validate_AcceptsAValidRequest()
    {
        var result = _sut.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsMissingName(string name)
    {
        var result = _sut.Validate(ValidRequest() with { Name = name });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Name is required.");
    }

    [Fact]
    public void Validate_RejectsNameShorterThanTwoCharacters()
    {
        var result = _sut.Validate(ValidRequest() with { Name = "A" });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Name must be between 2 and 100 characters.");
    }

    [Fact]
    public void Validate_RejectsNameLongerThanOneHundredCharacters()
    {
        var result = _sut.Validate(ValidRequest() with { Name = new string('a', 101) });

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Validate_RejectsAgeOutsideAllowedRange(int age)
    {
        var result = _sut.Validate(ValidRequest() with { Age = age });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Age must be between 0 and 120.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void Validate_AcceptsAgeBoundaries(int age)
    {
        var result = _sut.Validate(ValidRequest() with { Age = age });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RejectsMissingCity()
    {
        var result = _sut.Validate(ValidRequest() with { City = "  " });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("City is required.");
    }

    [Fact]
    public void Validate_RejectsMissingState()
    {
        var result = _sut.Validate(ValidRequest() with { State = string.Empty });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("State is required.");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    public void Validate_RejectsPincodeOutsideLengthRange(string pincode)
    {
        var result = _sut.Validate(ValidRequest() with { Pincode = pincode });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Pincode must be between 4 and 10 characters.");
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("1234567890")]
    public void Validate_AcceptsPincodeLengthBoundaries(string pincode)
    {
        var result = _sut.Validate(ValidRequest() with { Pincode = pincode });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_IgnoresSurroundingWhitespaceWhenMeasuringLength()
    {
        var result = _sut.Validate(ValidRequest() with { Pincode = "   600001   " });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReportsEveryInvalidFieldAtOnce()
    {
        var request = new CreateUserRequest(string.Empty, 200, string.Empty, string.Empty, "1");

        var result = _sut.Validate(request);

        result.Errors.Select(error => error.PropertyName)
            .Should().BeEquivalentTo("Name", "Age", "City", "State", "Pincode");
    }
}
