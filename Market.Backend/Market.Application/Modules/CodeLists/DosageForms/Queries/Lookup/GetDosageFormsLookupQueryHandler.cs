namespace Market.Application.Modules.CodeLists.DosageForms.Queries.Lookup;

/// <summary>
/// Polica (#6): returns all dosage forms that are not deleted, sorted by name, as dropdown options.
/// </summary>
public sealed class GetDosageFormsLookupQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetDosageFormsLookupQuery, List<DosageFormLookupQueryDto>>
{
    public async Task<List<DosageFormLookupQueryDto>> Handle(GetDosageFormsLookupQuery request, CancellationToken ct)
    {
        return await ctx.DosageForms
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new DosageFormLookupQueryDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(ct);
    }
}
