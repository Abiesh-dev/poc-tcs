namespace UserDirectory.Domain.Entities;

public static class UserConstraints
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;

    public const int MinAge = 0;
    public const int MaxAge = 120;

    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;

    public const int PincodeMinLength = 4;
    public const int PincodeMaxLength = 10;
}
