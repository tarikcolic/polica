using Market.Domain.Common;

namespace Market.Domain.Entities.CodeLists;

/// <summary>
/// Polica (#7): a city, e.g. "Sarajevo". A lookup table that pharmacies (#7) and suppliers (#9) point to
/// with a CityId foreign key, so the lists can be filtered by city and every address uses the same spelling.
/// </summary>
/// <remarks>
/// Supporting entity: it does not count toward the minimum of 10 entities (course rule 9), because it is a
/// simple lookup table with one attribute.
///
/// The starter had no City table (only a free-text City string on the user's profile, which stays as it is),
/// so it is created here, in the same migration as Pharmacy. The cities are reference data that every
/// environment needs, so they are seeded with the migration (StaticDataSeeder). There is no admin screen for
/// them: the API only offers a lookup for dropdowns (CitiesController).
///
/// Inherits <see cref="BaseEntity"/> (Id, IsDeleted, CreatedAtUtc, ModifiedAtUtc), like the other code lists.
/// </remarks>
public class CityEntity : BaseEntity
{
    /// <summary>
    /// Name of the city, e.g. "Banja Luka". Required, unique (unique index in CityEntityConfiguration).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Single source of truth for the limit (EF column size nvarchar(100)).
    /// </summary>
    public static class Constraints
    {
        public const int NameMaxLength = 100;
    }
}
