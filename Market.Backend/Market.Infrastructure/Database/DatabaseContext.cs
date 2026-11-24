using Market.Application.Abstractions;

namespace Market.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    public DbSet<MarketUserEntity> Users => Set<MarketUserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    // Polica (#6): code lists (tables Manufacturers, DosageForms, WriteOffReasons). The table mapping
    // is in Configurations/CodeLists, the seeded rows in Seeders/StaticDataSeeder and DynamicDataSeeder.
    public DbSet<ManufacturerEntity> Manufacturers => Set<ManufacturerEntity>();
    public DbSet<DosageFormEntity> DosageForms => Set<DosageFormEntity>();
    public DbSet<WriteOffReasonEntity> WriteOffReasons => Set<WriteOffReasonEntity>();

    // Polica (#7): tables Cities and Pharmacies. Mapping in Configurations/CodeLists/CityEntityConfiguration and
    // Configurations/Pharmacies; cities are seeded in StaticDataSeeder, demo pharmacies in DynamicDataSeeder.
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<PharmacyEntity> Pharmacies => Set<PharmacyEntity>();

    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock) : base(options)
    {
        _clock = clock;
    }
}
