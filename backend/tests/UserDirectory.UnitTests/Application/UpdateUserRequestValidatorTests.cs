using FluentAssertions;
using UserDirectory.Application.Users.Dtos;
using UserDirectory.Application.Users.Validation;
using Xunit;

namespace UserDirectory.UnitTests.Application;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _sut = new();

    private static UpdateUserRequest ValidRequest() =>
        new("Ada Lovelace", 36, "London", "Greater London", "600001");

    [Fact]
    public void Validate_AcceptsAValidRequest()
    {
        var result = _sut.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RejectsMissingName()
    {
        var result = _sut.Validate(ValidRequest() with { Name = string.Empty });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Name is required.");
    }

    [Fact]
    public void Validate_RejectsAgeAboveMaximum()
    {
        var result = _sut.Validate(ValidRequest() with { Age = 121 });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Age must be between 0 and 120.");
    }

    [Fact]
    public void Validate_RejectsShortPincode()
    {
        var result = _sut.Validate(ValidRequest() with { Pincode = "12" });

        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Pincode must be between 4 and 10 characters.");
    }
}
