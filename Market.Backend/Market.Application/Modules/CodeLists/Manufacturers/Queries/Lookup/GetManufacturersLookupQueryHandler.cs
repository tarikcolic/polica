namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.Lookup;

/// <summary>
/// Polica (#6): returns all manufacturers that are not deleted, sorted by name, as dropdown options.
/// </summary>
public sealed class GetManufacturersLookupQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetManufacturersLookupQuery, List<ManufacturerLookupQueryDto>>
{
    public async Task<List<ManufacturerLookupQueryDto>> Handle(GetManufacturersLookupQuery request, CancellationToken ct)
    {
        // One SELECT Id, Name ... ORDER BY Name; the global filter removes soft-deleted rows.
        return await ctx.Manufacturers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ManufacturerLookupQueryDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(ct);
    }
}
