namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.List;

/// <summary>
/// Polica (#6): one page of write-off reasons, sorted by name, optionally filtered by part of the name
/// (same query shape as ListManufacturersQueryHandler).
/// </summary>
public sealed class ListWriteOffReasonsQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListWriteOffReasonsQuery, PageResult<ListWriteOffReasonsQueryDto>>
{
    public async Task<PageResult<ListWriteOffReasonsQueryDto>> Handle(ListWriteOffReasonsQuery request, CancellationToken ct)
    {
        var query = ctx.WriteOffReasons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }

        // Stable order (Name, then Id) and a projection into the DTO; runs as COUNT + one page query.
        var projected = query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new ListWriteOffReasonsQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                RequiresNote = x.RequiresNote
            });

        return await PageResult<ListWriteOffReasonsQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
