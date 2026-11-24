namespace Market.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<MarketUserEntity> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
    DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; }

    // Polica (#6): code lists maintained by the chain administrator. Handlers in the Application layer
    // see only this interface, so every table a handler queries needs a DbSet here as well as in
    // DatabaseContext (Infrastructure), which implements it.
    DbSet<ManufacturerEntity> Manufacturers { get; }
    DbSet<DosageFormEntity> DosageForms { get; }
    DbSet<WriteOffReasonEntity> WriteOffReasons { get; }

    // Polica (#7): cities (lookup for pharmacies, later suppliers in #9) and the pharmacies of the chain.
    DbSet<CityEntity> Cities { get; }
    DbSet<PharmacyEntity> Pharmacies { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
