using FluentValidation;
using UserDirectory.Application.Users.Dtos;

namespace UserDirectory.Application.Users.Validation;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name).ApplyNameRules();
        RuleFor(x => x.Age).ApplyAgeRules();
        RuleFor(x => x.City).ApplyCityRules();
        RuleFor(x => x.State).ApplyStateRules();
        RuleFor(x => x.Pincode).ApplyPincodeRules();
    }
}
