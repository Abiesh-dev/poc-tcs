using UserDirectory.Application.Abstractions;

namespace UserDirectory.UnitTests.TestDoubles;

public sealed class FixedDateTimeProvider : IDateTimeProvider
{
    public FixedDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}
