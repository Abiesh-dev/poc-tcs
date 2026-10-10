namespace UserDirectory.Application.Users.Dtos;

public sealed record UserResponse(
    int Id,
    string Name,
    int Age,
    string City,
    string State,
    string Pincode,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
