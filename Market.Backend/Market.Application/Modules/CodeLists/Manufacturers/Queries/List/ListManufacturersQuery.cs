namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.List;

/// <summary>
/// Polica (#6): GET api/manufacturers?search=...&amp;paging.page=1&amp;paging.pageSize=10.
/// One page of manufacturers for the admin list screen.
/// </summary>
/// <remarks>
/// BasePagedQuery adds <c>Paging</c> (page number and page size, clamped by PageRequest), so this query
/// only declares its own filter. The controller binds it from the query string ([FromQuery]).
/// </remarks>
public sealed class ListManufacturersQuery : BasePagedQuery<ListManufacturersQueryDto>
{
    /// <summary>Optional: part of the name, e.g. "pli" finds "Pliva".</summary>
    public string? Search { get; init; }
}
