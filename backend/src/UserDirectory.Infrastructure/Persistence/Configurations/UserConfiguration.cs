using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    // SQLite stores timestamps as text and loses DateTimeKind, so values are
    // re-tagged as UTC on read to keep the API emitting ISO-8601 UTC strings.
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        value => value.ToUniversalTime(),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedOnAdd();

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(UserConstraints.NameMaxLength)
            .UseCollation("NOCASE");

        builder.Property(user => user.Age)
            .IsRequired();

        builder.Property(user => user.City)
            .IsRequired()
            .HasMaxLength(UserConstraints.CityMaxLength);

        builder.Property(user => user.State)
            .IsRequired()
            .HasMaxLength(UserConstraints.StateMaxLength);

        builder.Property(user => user.Pincode)
            .IsRequired()
            .HasMaxLength(UserConstraints.PincodeMaxLength)
            .UseCollation("NOCASE");

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired()
            .HasConversion(UtcConverter);

        builder.Property(user => user.UpdatedAtUtc)
            .IsRequired()
            .HasConversion(UtcConverter);

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Users_Age",
            $"[Age] >= {UserConstraints.MinAge} AND [Age] <= {UserConstraints.MaxAge}"));

        builder.HasIndex(user => new { user.Name, user.Pincode })
            .IsUnique()
            .HasDatabaseName("IX_Users_Name_Pincode");
    }
}
