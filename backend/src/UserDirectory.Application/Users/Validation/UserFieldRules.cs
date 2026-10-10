using FluentValidation;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Users.Validation;

internal static class UserFieldRules
{
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .Must(value => HasTrimmedLengthBetween(value, UserConstraints.NameMinLength, UserConstraints.NameMaxLength))
            .WithMessage($"Name must be between {UserConstraints.NameMinLength} and {UserConstraints.NameMaxLength} characters.");

    public static IRuleBuilderOptions<T, int> ApplyAgeRules<T>(this IRuleBuilderInitial<T, int> rule) =>
        rule.InclusiveBetween(UserConstraints.MinAge, UserConstraints.MaxAge)
            .WithMessage($"Age must be between {UserConstraints.MinAge} and {UserConstraints.MaxAge}.");

    public static IRuleBuilderOptions<T, string> ApplyCityRules<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("City is required.")
            .Must(value => HasTrimmedLengthBetween(value, 1, UserConstraints.CityMaxLength))
            .WithMessage($"City must not exceed {UserConstraints.CityMaxLength} characters.");

    public static IRuleBuilderOptions<T, string> ApplyStateRules<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("State is required.")
            .Must(value => HasTrimmedLengthBetween(value, 1, UserConstraints.StateMaxLength))
            .WithMessage($"State must not exceed {UserConstraints.StateMaxLength} characters.");

    public static IRuleBuilderOptions<T, string> ApplyPincodeRules<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Pincode is required.")
            .Must(value => HasTrimmedLengthBetween(value, UserConstraints.PincodeMinLength, UserConstraints.PincodeMaxLength))
            .WithMessage($"Pincode must be between {UserConstraints.PincodeMinLength} and {UserConstraints.PincodeMaxLength} characters.");

    private static bool HasTrimmedLengthBetween(string? value, int minLength, int maxLength)
    {
        var length = (value ?? string.Empty).Trim().Length;
        return length >= minLength && length <= maxLength;
    }
}
