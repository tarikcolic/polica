namespace Market.Infrastructure.Database.Configurations.CodeLists;

/// <summary>
/// Polica (#7): EF Core Fluent API mapping of <see cref="CityEntity"/> to the "Cities" table.
/// Picked up automatically by ApplyConfigurationsFromAssembly in DatabaseConfiguration.OnModelCreating.
/// The seeded cities are in StaticDataSeeder.
/// </summary>
public sealed class CityEntityConfiguration : IEntityTypeConfiguration<CityEntity>
{
    public void Configure(EntityTypeBuilder<CityEntity> b)
    {
        b.ToTable("Cities");

        b.HasKey(x => x.Id);

        // nvarchar(100) NOT NULL
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(CityEntity.Constraints.NameMaxLength);

        // One row per city name, so a dropdown never shows "Sarajevo" twice. Filtered like the other code lists
        // (unique only among rows that are not soft-deleted).
        b.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
