namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.List;

/// <summary>
/// Polica (#6): GET api/write-off-reasons?search=...&amp;paging.page=1&amp;paging.pageSize=10.
/// One page of write-off reasons for the admin list screen; Paging comes from BasePagedQuery.
/// </summary>
public sealed class ListWriteOffReasonsQuery : BasePagedQuery<ListWriteOffReasonsQueryDto>
{
    /// <summary>Optional: part of the name, e.g. "rok" finds "Istekao rok trajanja".</summary>
    public string? Search { get; init; }
}
