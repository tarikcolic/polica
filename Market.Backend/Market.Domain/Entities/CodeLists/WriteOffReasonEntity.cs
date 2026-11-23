using Market.Domain.Common;

namespace Market.Domain.Entities.CodeLists;

/// <summary>
/// Polica (#6): why stock is written off, e.g. "Istekao rok trajanja" or "Oštećeno pakovanje".
/// One of the three code lists (šifarnici) that the chain administrator maintains on
/// /admin/code-lists/write-off-reasons.
/// </summary>
/// <remarks>
/// Supporting entity, not counted toward the minimum of 10 entities. WriteOff (#20) will point to it
/// with a WriteOffReasonId foreign key; the pharmacist picks a reason when proposing a write-off.
///
/// The common reasons are seeded in every environment (StaticDataSeeder → part of the migration),
/// because the write-off flow cannot work without at least one reason.
/// Deleting is a soft delete, the same as for <see cref="ManufacturerEntity"/>.
/// </remarks>
public class WriteOffReasonEntity : BaseEntity
{
    /// <summary>
    /// Name of the reason, e.g. "Oštećeno pakovanje". Required, unique among the reasons that are not
    /// deleted (case-insensitive).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// True when a write-off with this reason must also have a note that explains it, e.g. "Ostalo"
    /// (the reason alone says nothing). The rule itself is enforced by the write-off command in #20;
    /// this code list only stores the setting.
    /// </summary>
    public bool RequiresNote { get; set; }

    /// <summary>
    /// Single source of truth for the limits (EF column size and FluentValidation rules).
    /// </summary>
    public static class Constraints
    {
        public const int NameMaxLength = 100;
    }
}
