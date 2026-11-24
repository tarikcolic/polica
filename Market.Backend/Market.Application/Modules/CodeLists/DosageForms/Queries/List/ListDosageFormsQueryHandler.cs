namespace Market.Application.Modules.CodeLists.DosageForms.Queries.List;

/// <summary>
/// Polica (#6): one page of dosage forms, sorted by name, optionally filtered by part of the name
/// (same query shape as ListManufacturersQueryHandler).
/// </summary>
public sealed class ListDosageFormsQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListDosageFormsQuery, PageResult<ListDosageFormsQueryDto>>
{
    public async Task<PageResult<ListDosageFormsQueryDto>> Handle(ListDosageFormsQuery request, CancellationToken ct)
    {
        var query = ctx.DosageForms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }

        // Stable order (Name, then Id) and a projection into the DTO; runs as COUNT + one page query.
        var projected = query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new ListDosageFormsQueryDto
            {
                Id = x.Id,
                Name = x.Name
            });

        return await PageResult<ListDosageFormsQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
