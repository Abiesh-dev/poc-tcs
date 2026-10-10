namespace UserDirectory.Application.Users.Dtos;

public sealed record CreateUserRequest(
    string Name,
    int Age,
    string City,
    string State,
    string Pincode);
