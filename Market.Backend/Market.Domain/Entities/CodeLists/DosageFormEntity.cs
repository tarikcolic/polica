using Market.Domain.Common;

namespace Market.Domain.Entities.CodeLists;

/// <summary>
/// Polica (#6): the pharmaceutical form of a medication, e.g. "Tableta", "Sirup", "Injekcija".
/// One of the three code lists (šifarnici) that the chain administrator maintains on
/// /admin/code-lists/dosage-forms.
/// </summary>
/// <remarks>
/// Supporting entity, not counted toward the minimum of 10 entities. Medication (#10) will point to it
/// with a DosageFormId foreign key, and the catalog search filters by it.
///
/// The five common forms are seeded in every environment (StaticDataSeeder → part of the migration),
/// because a medication cannot be entered without a dosage form.
/// Deleting is a soft delete, the same as for <see cref="ManufacturerEntity"/>.
/// </remarks>
public class DosageFormEntity : BaseEntity
{
    /// <summary>
    /// Name of the form, e.g. "Tableta". Required, unique among the dosage forms that are not deleted
    /// (case-insensitive).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Single source of truth for the limits (EF column size and FluentValidation rules).
    /// </summary>
    public static class Constraints
    {
        public const int NameMaxLength = 50;
    }
}
