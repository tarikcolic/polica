namespace Market.Infrastructure.Database.Seeders;

/// <summary>
/// Static (reference) data that every environment needs, including production.
/// It is written into the migration with <c>HasData</c>, so it needs fixed Ids and fixed dates
/// (a value such as <c>DateTime.UtcNow</c> would change the model and create a new migration every time).
/// Demo data for development belongs in <see cref="DynamicDataSeeder"/>.
/// </summary>
/// <remarks>
/// Polica (#4): the starter seeded currencies and the EUR rate here; both were removed together with the
/// Finance module, so the method is empty for now. It stays as the single place for reference data:
/// the single AppSettings row (Id = 1, issue #11) is the first planned entry.
/// It is called from <see cref="DatabaseConfiguration"/> (OnModelCreating), so anything added here
/// ends up in the next migration.
/// </remarks>
public static class StaticDataSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
    }
}
