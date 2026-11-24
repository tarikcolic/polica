namespace Market.Infrastructure.Database.Configurations.CodeLists;

/// <summary>
/// Polica (#6): EF Core Fluent API mapping of <see cref="DosageFormEntity"/> to the "DosageForms" table.
/// Picked up automatically by ApplyConfigurationsFromAssembly in DatabaseConfiguration.OnModelCreating.
/// The seeded rows (Tableta, Kapsula...) are in StaticDataSeeder, not here.
/// </summary>
public sealed class DosageFormEntityConfiguration : IEntityTypeConfiguration<DosageFormEntity>
{
    public void Configure(EntityTypeBuilder<DosageFormEntity> b)
    {
        b.ToTable("DosageForms");

        b.HasKey(x => x.Id);

        // nvarchar(50) NOT NULL; the same limit is used by the validators (Constraints class).
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DosageFormEntity.Constraints.NameMaxLength);

        // Unique among rows that are not soft-deleted; last guard behind the handler check
        // (see ManufacturerEntityConfiguration for the full explanation).
        b.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
