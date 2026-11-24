using Market.Application.Modules.CodeLists.Cities.Queries.Lookup;

namespace Market.API.Controllers;

/// <summary>
/// Polica (#7): cities, read-only. The cities are seeded with the migration (StaticDataSeeder) and have no admin
/// screen, so the only endpoint is the lookup for dropdowns (pharmacy form and filter, supplier form in #9).
/// Any logged-in user (fallback policy in Market.API/DependencyInjection.cs).
/// </summary>
[ApiController]
[Route("api/cities")]
public sealed class CitiesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// All cities as (id, name), sorted by name. Not paged.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<CityLookupQueryDto>), StatusCodes.Status200OK)]
    public async Task<List<CityLookupQueryDto>> Lookup(CancellationToken ct)
    {
        return await sender.Send(new GetCitiesLookupQuery(), ct);
    }
}
