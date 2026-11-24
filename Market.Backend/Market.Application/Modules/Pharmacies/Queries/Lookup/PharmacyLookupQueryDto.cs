namespace Market.Application.Modules.Pharmacies.Queries.Lookup;

/// <summary>
/// Polica (#7): one dropdown option. The UI shows "Code – Name" (e.g. "AP-01 – Apoteka Centar"), because two
/// pharmacies may have the same name; the code is unique.
/// </summary>
public sealed class PharmacyLookupQueryDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}
