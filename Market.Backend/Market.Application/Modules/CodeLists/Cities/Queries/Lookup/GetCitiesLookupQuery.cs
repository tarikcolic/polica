namespace Market.Application.Modules.CodeLists.Cities.Queries.Lookup;

/// <summary>
/// Polica (#7): GET api/cities/lookup. Every city as (Id, Name), sorted by name, for a dropdown: the city of a
/// pharmacy (pharmacy form and the city filter of the pharmacy list), later the city of a supplier (#9).
/// </summary>
/// <remarks>
/// Not paged: a dropdown needs all options at once and there are only a few cities (seeded, StaticDataSeeder).
/// Cities have no admin screen, so this is the only City endpoint.
/// </remarks>
public sealed class GetCitiesLookupQuery : IRequest<List<CityLookupQueryDto>>
{
}
