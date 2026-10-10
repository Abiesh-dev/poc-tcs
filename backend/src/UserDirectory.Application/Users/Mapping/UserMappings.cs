using UserDirectory.Application.Users.Dtos;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Users.Mapping;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id,
        user.Name,
        user.Age,
        user.City,
        user.State,
        user.Pincode,
        user.CreatedAtUtc,
        user.UpdatedAtUtc);
}
