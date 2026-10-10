namespace UserDirectory.Application.Users.Dtos;

public sealed record UpdateUserRequest(
    string Name,
    int Age,
    string City,
    string State,
    string Pincode);
