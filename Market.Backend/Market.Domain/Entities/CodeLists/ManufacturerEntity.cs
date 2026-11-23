using Market.Domain.Common;

namespace Market.Domain.Entities.CodeLists;

/// <summary>
/// Polica (#6): a medication manufacturer, e.g. "Bosnalijek". One of the three code lists (šifarnici)
/// that the chain administrator maintains on /admin/code-lists/manufacturers.
/// </summary>
/// <remarks>
/// Supporting entity: it does not count toward the minimum of 10 entities (course rule 9), because it is
/// a simple lookup table. Medication (#10) will point to it with a ManufacturerId foreign key.
///
/// Inherits <see cref="BaseEntity"/> (Id, IsDeleted, CreatedAtUtc, ModifiedAtUtc). Deleting a manufacturer
/// is therefore a SOFT delete: DatabaseContext.ApplyAuditAndSoftDelete turns the DELETE into
/// "IsDeleted = 1", and the global query filter hides the row from every later query.
/// </remarks>
public class ManufacturerEntity : BaseEntity
{
    /// <summary>
    /// Name shown in the catalog, e.g. "Bosnalijek". Required and unique among the manufacturers that are
    /// not deleted (case-insensitive): checked in the Create/Update handlers, guarded by a unique index.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Country of the manufacturer, e.g. "Bosna i Hercegovina". Optional: null when not entered.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Single source of truth for the limits. Used in two places, so they can never drift apart:
    /// the EF configuration (column size nvarchar(100)) and the FluentValidation validators
    /// (a readable 400 error before the database is touched).
    /// </summary>
    public static class Constraints
    {
        public const int NameMaxLength = 100;
        public const int CountryMaxLength = 100;
    }
}
