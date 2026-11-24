namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.Lookup;

/// <summary>
/// Polica (#6): returns all write-off reasons that are not deleted, sorted by name, as dropdown options.
/// </summary>
public sealed class GetWriteOffReasonsLookupQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetWriteOffReasonsLookupQuery, List<WriteOffReasonLookupQueryDto>>
{
    public async Task<List<WriteOffReasonLookupQueryDto>> Handle(GetWriteOffReasonsLookupQuery request, CancellationToken ct)
    {
        return await ctx.WriteOffReasons
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new WriteOffReasonLookupQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                RequiresNote = x.RequiresNote
            })
            .ToListAsync(ct);
    }
}
