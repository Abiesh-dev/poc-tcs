namespace UserDirectory.Domain.Exceptions;

public sealed class DuplicateUserException : DomainException
{
    public DuplicateUserException(string name, string pincode)
        : base($"A user named '{name}' already exists for pincode '{pincode}'.")
    {
        Name = name;
        Pincode = pincode;
    }

    public string Name { get; }

    public string Pincode { get; }
}
