using Market.Domain.Common;
using Market.Domain.Entities.CodeLists;

namespace Market.Domain.Entities.Pharmacies;

/// <summary>
/// Polica (#7): one pharmacy (apoteka) of the chain, e.g. "AP-01 Apoteka Centar, Sarajevo".
/// The chain administrator maintains them on /admin/pharmacies.
/// </summary>
/// <remarks>
/// Counted entity (C1 in the ER diagram). Almost everything later in the flow belongs to one pharmacy:
/// employees (#8, User.PharmacyId), purchase orders (#12), stock items (#13), dispenses (#15) and transfers
/// between pharmacies (#18, #19).
///
/// Design decision: there is NO ManagerId column here. The manager of a pharmacy is the user with the
/// manager role (IsManager) whose PharmacyId equals this pharmacy's Id; the assignment is made on the employee
/// screen (#8). Two foreign keys pointing at each other (Pharmacy → User and User → Pharmacy) would store the
/// same fact twice and could drift apart, and SQL Server would reject the two cascade paths unless both were
/// Restrict. So: one rule, one place to change it.
///
/// Deleting a pharmacy is not offered; it is deactivated instead (<see cref="IsActive"/> = false), because its
/// history (orders, stock movements) must stay readable. BaseEntity still gives it IsDeleted and the audit columns.
/// </remarks>
public class PharmacyEntity : BaseEntity
{
    /// <summary>Name shown in the UI, e.g. "Apoteka Centar". Required, max 100. Not unique (the Code is).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short business code, e.g. "AP-01". Required, max 10, only letters A-Z, digits and "-".
    /// Always stored in UPPERCASE (the handlers convert it) and unique among the pharmacies that are not deleted.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Street and number, e.g. "Ferhadija 12". Required, max 200. The city is a separate FK.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Contact phone, e.g. "+387 33 123 456". Required, max 30.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>FK to <see cref="CityEntity"/>. Restrict on delete: a city used by a pharmacy cannot be removed.</summary>
    public int CityId { get; set; }

    /// <summary>Navigation property to the city, used by queries (e.g. x.City.Name in the list projection).</summary>
    public CityEntity City { get; set; } = null!;

    /// <summary>
    /// false = deactivated: the pharmacy keeps its data but is no longer offered in dropdowns (employee
    /// assignment #8, transfer destination #18). Changed only through the activate/deactivate endpoints, never
    /// through the general update, so the rule added in #13 ("cannot deactivate while it holds stock") has
    /// exactly one place to live. New pharmacies are active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Single source of truth for the limits. Used by the EF configuration (column sizes) and by the
    /// FluentValidation validators (readable 400 errors before the database is touched).
    /// </summary>
    public static class Constraints
    {
        public const int NameMaxLength = 100;
        public const int CodeMaxLength = 10;
        public const int AddressMaxLength = 200;
        public const int PhoneMaxLength = 30;
    }
}
