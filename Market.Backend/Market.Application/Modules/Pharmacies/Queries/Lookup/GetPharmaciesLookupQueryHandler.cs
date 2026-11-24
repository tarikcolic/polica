namespace Market.Application.Modules.Pharmacies.Queries.Lookup;

/// <summary>
/// Polica (#7): returns the active pharmacies, sorted by code, as dropdown options.
/// </summary>
public sealed class GetPharmaciesLookupQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetPharmaciesLookupQuery, List<PharmacyLookupQueryDto>>
{
    public async Task<List<PharmacyLookupQueryDto>> Handle(GetPharmaciesLookupQuery request, CancellationToken ct)
    {
        // WHERE IsActive = 1 is the "inactive pharmacy is not offered in dropdowns" rule of #7.
        // (IsDeleted = 0 is added by the global query filter.)
        return await ctx.Pharmacies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new PharmacyLookupQueryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            })
            .ToListAsync(ct);
    }
}
