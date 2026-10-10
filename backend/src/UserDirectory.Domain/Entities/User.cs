using UserDirectory.Domain.Exceptions;

namespace UserDirectory.Domain.Entities;

public class User
{
    private User()
    {
    }

    public User(string name, int age, string city, string state, string pincode, DateTime createdAtUtc)
    {
        Apply(name, age, city, state, pincode);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int Age { get; private set; }

    public string City { get; private set; } = null!;

    public string State { get; private set; } = null!;

    public string Pincode { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void UpdateDetails(string name, int age, string city, string state, string pincode, DateTime updatedAtUtc)
    {
        Apply(name, age, city, state, pincode);
        UpdatedAtUtc = updatedAtUtc;
    }

    // Every field is validated before any is assigned so that a rejected change
    // never leaves the entity in a partially updated state.
    private void Apply(string name, int age, string city, string state, string pincode)
    {
        var validatedName = NormalizeText(
            name,
            nameof(Name),
            UserConstraints.NameMinLength,
            UserConstraints.NameMaxLength);

        if (age is < UserConstraints.MinAge or > UserConstraints.MaxAge)
        {
            throw new InvalidUserDataException(
                nameof(Age),
                $"Age must be between {UserConstraints.MinAge} and {UserConstraints.MaxAge}.");
        }

        var validatedCity = NormalizeText(city, nameof(City), 1, UserConstraints.CityMaxLength);
        var validatedState = NormalizeText(state, nameof(State), 1, UserConstraints.StateMaxLength);
        var validatedPincode = NormalizeText(
            pincode,
            nameof(Pincode),
            UserConstraints.PincodeMinLength,
            UserConstraints.PincodeMaxLength);

        Name = validatedName;
        Age = age;
        City = validatedCity;
        State = validatedState;
        Pincode = validatedPincode;
    }

    private static string NormalizeText(string? value, string field, int minLength, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length < minLength || trimmed.Length > maxLength)
        {
            throw new InvalidUserDataException(
                field,
                $"{field} must be between {minLength} and {maxLength} characters.");
        }

        return trimmed;
    }
}
