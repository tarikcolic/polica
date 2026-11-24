namespace Market.Infrastructure.Database.Configurations.Pharmacies;

/// <summary>
/// Polica (#7): EF Core Fluent API mapping of <see cref="PharmacyEntity"/> to the "Pharmacies" table.
/// Picked up automatically by ApplyConfigurationsFromAssembly in DatabaseConfiguration.OnModelCreating.
/// </summary>
public sealed class PharmacyEntityConfiguration : IEntityTypeConfiguration<PharmacyEntity>
{
    public void Configure(EntityTypeBuilder<PharmacyEntity> b)
    {
        b.ToTable("Pharmacies");

        b.HasKey(x => x.Id);

        // Column sizes come from PharmacyEntity.Constraints, the same numbers the validators use.
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PharmacyEntity.Constraints.NameMaxLength);

        b.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(PharmacyEntity.Constraints.CodeMaxLength);

        b.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(PharmacyEntity.Constraints.AddressMaxLength);

        b.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(PharmacyEntity.Constraints.PhoneMaxLength);

        // bit NOT NULL, without HasDefaultValue: with a database default EF Core would not send "false"
        // (the CLR default), see UserEntityConfiguration. The entity's own default (true) is used instead.
        b.Property(x => x.IsActive)
            .IsRequired();

        // Unique code. The Create/Update handlers check it first and return a field error on "code"; this index
        // is the last guard when two requests pass that check at the same time (the second INSERT then fails
        // with DbUpdateException → 409). Filtered to rows that are not soft-deleted, like every other unique
        // index in the project, so it matches the handler check (the global query filter hides deleted rows).
        b.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Many pharmacies → one city. Restrict: SQL Server refuses to delete a city that a pharmacy still uses
        // (instead of Cascade, which would silently delete the pharmacies with it).
        b.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
