namespace UserDirectory.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Path { get; set; } = "data/app.db";

    public bool SeedSampleData { get; set; }
}
