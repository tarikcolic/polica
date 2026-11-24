namespace Market.Application.Modules.Pharmacies.Queries.List;

/// <summary>
/// Polica (#7): one row of the pharmacies table. A DTO, so the API never returns the entity itself.
/// </summary>
public sealed class ListPharmaciesQueryDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string Phone { get; init; }

    /// <summary>Name of the city (joined from Cities), so the table does not need a second request.</summary>
    public required string CityName { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>
    /// "Firstname Lastname" of the pharmacy's manager, derived (not stored): the user with IsManager whose
    /// PharmacyId is this pharmacy. Always null in #7, because users get PharmacyId only in #8; the list shows "–".
    /// </summary>
    public string? ManagerName { get; init; }
}
