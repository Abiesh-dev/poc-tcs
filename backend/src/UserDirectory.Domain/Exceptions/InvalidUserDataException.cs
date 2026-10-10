namespace UserDirectory.Domain.Exceptions;

public sealed class InvalidUserDataException : DomainException
{
    public InvalidUserDataException(string field, string message) : base(message)
    {
        Field = field;
    }

    public string Field { get; }
}
