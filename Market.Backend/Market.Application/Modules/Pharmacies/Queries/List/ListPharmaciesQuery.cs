namespace Market.Application.Modules.Pharmacies.Queries.List;

/// <summary>
/// Polica (#7): GET api/pharmacies?search=...&amp;cityId=1&amp;isActive=true&amp;paging.page=1&amp;paging.pageSize=10.
/// One page of pharmacies for the admin list screen.
/// </summary>
/// <remarks>
/// BasePagedQuery adds <c>Paging</c> (page number and size). Every filter is optional: a filter that is not sent
/// (null) is not applied. The controller binds the query from the query string ([FromQuery]).
/// </remarks>
public sealed class ListPharmaciesQuery : BasePagedQuery<ListPharmaciesQueryDto>
{
    /// <summary>Optional: part of the name or of the code, e.g. "centar" or "ap-0".</summary>
    public string? Search { get; init; }

    /// <summary>Optional: only pharmacies in this city.</summary>
    public int? CityId { get; init; }

    /// <summary>Optional: true = only active, false = only inactive, null = all.</summary>
    public bool? IsActive { get; init; }
}
