namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.List;

/// <summary>
/// Polica (#6): returns one page of manufacturers, sorted by name, optionally filtered by part of the name.
/// </summary>
public sealed class ListManufacturersQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListManufacturersQuery, PageResult<ListManufacturersQueryDto>>
{
    public async Task<PageResult<ListManufacturersQueryDto>> Handle(ListManufacturersQuery request, CancellationToken ct)
    {
        // AsNoTracking: read-only query, EF does not need to remember the rows for a later save.
        // The global query filter (IsDeleted = 0) is applied automatically.
        var query = ctx.Manufacturers.AsNoTracking();

        // The query is only BUILT here (IQueryable); nothing runs until FromQueryableAsync below.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            // LIKE '%search%'; case-insensitive on SQL Server thanks to its default collation.
            query = query.Where(x => x.Name.Contains(search));
        }

        // ThenBy(Id) makes the order stable, so paging never repeats or skips a row between pages.
        // Select(...) is a projection: SQL reads only these columns straight into the DTO.
        var projected = query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new ListManufacturersQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                Country = x.Country
            });

        // Two SQL queries: COUNT(*) for the total, then OFFSET/FETCH for the requested page.
        return await PageResult<ListManufacturersQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
