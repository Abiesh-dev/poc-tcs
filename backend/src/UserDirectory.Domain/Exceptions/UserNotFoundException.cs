namespace UserDirectory.Domain.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(int userId)
        : base($"User with id {userId} was not found.")
    {
        UserId = userId;
    }

    public int UserId { get; }
}
