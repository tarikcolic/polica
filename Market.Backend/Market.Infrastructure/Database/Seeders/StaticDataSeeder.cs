namespace Market.Infrastructure.Database.Seeders;

/// <summary>
/// Static (reference) data that every environment needs, including production.
/// It is written into the migration with <c>HasData</c>, so it needs fixed Ids and fixed dates
/// (a value such as <c>DateTime.UtcNow</c> would change the model and create a new migration every time).
/// Demo data for development belongs in <see cref="DynamicDataSeeder"/>.
/// </summary>
/// <remarks>
/// Polica (#4): the starter seeded currencies and the EUR rate here; both were removed together with the
/// Finance module. This class stays as the single place for reference data.
/// It is called from <see cref="DatabaseConfiguration"/> (OnModelCreating), so anything added here
/// ends up in the next migration.
///
/// Polica (#6): dosage forms and write-off reasons are seeded here, because the application cannot work
/// without them in any environment: a medication (#10) needs a dosage form, a write-off (#20) needs a reason.
/// Manufacturers are NOT here: which manufacturers a chain buys from is business data the administrator
/// enters, so the demo manufacturers are in <see cref="DynamicDataSeeder"/> (Development and Testing only).
///
/// Ids: HasData inserts the rows with these explicit Ids (SQL Server IDENTITY_INSERT). SQL Server then moves
/// the identity counter past the highest inserted value, so a row the administrator creates later gets Id 6, 7...
/// The administrator may rename or delete a seeded row; that only changes the data, the migration stays as is.
/// </remarks>
public static class StaticDataSeeder
{
    /// <summary>
    /// Fixed CreatedAtUtc for every seeded row (BaseEntity requires it; see the note on fixed dates above).
    /// </summary>
    private static readonly DateTime SeededAtUtc = new(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedDosageForms(modelBuilder);
        SeedWriteOffReasons(modelBuilder);
    }

    /// <summary>
    /// The five forms listed in issue #6. Names are data shown in the UI, so they are in Bosnian.
    /// </summary>
    private static void SeedDosageForms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DosageFormEntity>().HasData(
            new DosageFormEntity { Id = 1, Name = "Tableta", CreatedAtUtc = SeededAtUtc },
            new DosageFormEntity { Id = 2, Name = "Kapsula", CreatedAtUtc = SeededAtUtc },
            new DosageFormEntity { Id = 3, Name = "Sirup", CreatedAtUtc = SeededAtUtc },
            new DosageFormEntity { Id = 4, Name = "Mast", CreatedAtUtc = SeededAtUtc },
            new DosageFormEntity { Id = 5, Name = "Injekcija", CreatedAtUtc = SeededAtUtc });
    }

    /// <summary>
    /// Common write-off reasons. RequiresNote = true where the reason alone does not explain what happened
    /// ("Ostalo"), or where the details matter later (a storage problem: which fridge, what temperature).
    /// </summary>
    private static void SeedWriteOffReasons(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WriteOffReasonEntity>().HasData(
            new WriteOffReasonEntity { Id = 1, Name = "Istekao rok trajanja", RequiresNote = false, CreatedAtUtc = SeededAtUtc },
            new WriteOffReasonEntity { Id = 2, Name = "Oštećeno pakovanje", RequiresNote = false, CreatedAtUtc = SeededAtUtc },
            new WriteOffReasonEntity { Id = 3, Name = "Povučena serija", RequiresNote = false, CreatedAtUtc = SeededAtUtc },
            new WriteOffReasonEntity { Id = 4, Name = "Neodgovarajuće čuvanje", RequiresNote = true, CreatedAtUtc = SeededAtUtc },
            new WriteOffReasonEntity { Id = 5, Name = "Ostalo", RequiresNote = true, CreatedAtUtc = SeededAtUtc });
    }
}
