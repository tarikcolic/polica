namespace Market.Application.Modules.CodeLists.Cities.Queries.Lookup;

/// <summary>
/// Polica (#7): one dropdown option: the value (Id, stored as CityId on a pharmacy) and the text (Name).
/// </summary>
public sealed class CityLookupQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
