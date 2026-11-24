namespace Market.Application.Modules.Pharmacies.Queries.GetById;

/// <summary>
/// Polica (#7): one pharmacy with every field the edit page needs. CityId selects the dropdown option; CityName
/// is for read-only views (e.g. the manager's own pharmacy from #8 on).
/// </summary>
public sealed class GetPharmacyByIdQueryDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string Phone { get; init; }
    public required int CityId { get; init; }
    public required string CityName { get; init; }
    public required bool IsActive { get; init; }
}
