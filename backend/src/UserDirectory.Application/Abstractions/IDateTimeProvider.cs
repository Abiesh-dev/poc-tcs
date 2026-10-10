namespace UserDirectory.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
