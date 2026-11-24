namespace Market.Infrastructure.Database.Configurations.CodeLists;

/// <summary>
/// Polica (#6): EF Core Fluent API mapping of <see cref="WriteOffReasonEntity"/> to the "WriteOffReasons" table.
/// Picked up automatically by ApplyConfigurationsFromAssembly in DatabaseConfiguration.OnModelCreating.
/// The seeded rows (Istekao rok trajanja, Ostalo...) are in StaticDataSeeder, not here.
/// </summary>
public sealed class WriteOffReasonEntityConfiguration : IEntityTypeConfiguration<WriteOffReasonEntity>
{
    public void Configure(EntityTypeBuilder<WriteOffReasonEntity> b)
    {
        b.ToTable("WriteOffReasons");

        b.HasKey(x => x.Id);

        // nvarchar(100) NOT NULL; the same limit is used by the validators (Constraints class).
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(WriteOffReasonEntity.Constraints.NameMaxLength);

        // bit NOT NULL. Deliberately no HasDefaultValue(...): with a database default EF Core does not send
        // a bool equal to the CLR default (false), so "false" could not be saved reliably
        // (same reason as in UserEntityConfiguration). The entity's own default (false) is enough.
        b.Property(x => x.RequiresNote)
            .IsRequired();

        // Unique among rows that are not soft-deleted; last guard behind the handler check
        // (see ManufacturerEntityConfiguration for the full explanation).
        b.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
