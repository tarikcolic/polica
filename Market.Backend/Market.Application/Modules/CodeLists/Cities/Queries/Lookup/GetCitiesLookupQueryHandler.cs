namespace Market.Application.Modules.CodeLists.Cities.Queries.Lookup;

/// <summary>
/// Polica (#7): returns all cities, sorted by name, as dropdown options.
/// </summary>
public sealed class GetCitiesLookupQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetCitiesLookupQuery, List<CityLookupQueryDto>>
{
    public async Task<List<CityLookupQueryDto>> Handle(GetCitiesLookupQuery request, CancellationToken ct)
    {
        // One SELECT Id, Name ... ORDER BY Name. AsNoTracking: read-only, nothing will be saved.
        return await ctx.Cities
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CityLookupQueryDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(ct);
    }
}
