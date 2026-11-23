namespace Market.Infrastructure.Database.Configurations.CodeLists;

/// <summary>
/// Polica (#6): EF Core Fluent API mapping of <see cref="ManufacturerEntity"/> to the "Manufacturers" table.
/// Picked up automatically by ApplyConfigurationsFromAssembly in DatabaseConfiguration.OnModelCreating.
/// </summary>
public sealed class ManufacturerEntityConfiguration : IEntityTypeConfiguration<ManufacturerEntity>
{
    public void Configure(EntityTypeBuilder<ManufacturerEntity> b)
    {
        b.ToTable("Manufacturers");

        b.HasKey(x => x.Id);

        // nvarchar(100) NOT NULL; the same limit is used by the validators (Constraints class).
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ManufacturerEntity.Constraints.NameMaxLength);

        // nvarchar(100) NULL: the country is optional.
        b.Property(x => x.Country)
            .HasMaxLength(ManufacturerEntity.Constraints.CountryMaxLength);

        // Unique name. The Create/Update handlers check it first and return a field error on "name";
        // this index is the last guard when two requests pass that check at the same time
        // (the second INSERT then fails with DbUpdateException → 409).
        // The filter makes it unique only among rows that are NOT deleted: delete is a soft delete,
        // so without the filter a deleted "Pliva" would block creating "Pliva" again.
        // SQL Server's default collation is case-insensitive, so "pliva" and "Pliva" collide here too.
        b.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
