namespace Market.Application.Modules.Pharmacies.Queries.List;

/// <summary>
/// Polica (#7): returns one page of pharmacies, sorted by code, filtered by search text, city and active state.
/// </summary>
public sealed class ListPharmaciesQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<ListPharmaciesQuery, PageResult<ListPharmaciesQueryDto>>
{
    public async Task<PageResult<ListPharmaciesQueryDto>> Handle(ListPharmaciesQuery request, CancellationToken ct)
    {
        // The list of all pharmacies is the administrator's screen (the endpoint has the Admin policy too).
        // A manager or pharmacist reads only their own pharmacy through GET api/pharmacies/{id} (#8).
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može pregledati sve apoteke.");

        // AsNoTracking: read-only. The global query filter (IsDeleted = 0) is applied automatically.
        // Nothing runs yet: the query is only BUILT here (IQueryable) and executed by FromQueryableAsync below.
        var query = ctx.Pharmacies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            // LIKE '%search%' on both columns; case-insensitive on SQL Server thanks to its default collation.
            query = query.Where(x => x.Name.Contains(search) || x.Code.Contains(search));
        }

        if (request.CityId is not null)
            query = query.Where(x => x.CityId == request.CityId);

        if (request.IsActive is not null)
            query = query.Where(x => x.IsActive == request.IsActive);

        // Sorted by code (AP-01, AP-02...), the order the chain uses on documents; ThenBy(Id) keeps the order
        // stable between pages. Select(...) is a projection: SQL reads only these columns, and x.City.Name becomes
        // an INNER JOIN on Cities inside the same query (no Include, no second query per row).
        var projected = query
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Select(x => new ListPharmaciesQueryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                CityName = x.City.Name,
                IsActive = x.IsActive,
                // #8 replaces this with a subquery over Users (IsManager && PharmacyId == x.Id), once users have
                // a PharmacyId. Pharmacy itself has no ManagerId column (see PharmacyEntity).
                ManagerName = null
            });

        // Two SQL queries: COUNT(*) for the total, then OFFSET/FETCH for the requested page.
        return await PageResult<ListPharmaciesQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
