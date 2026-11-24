namespace Market.Application.Modules.CodeLists.DosageForms.Queries.List;

/// <summary>
/// Polica (#6): GET api/dosage-forms?search=...&amp;paging.page=1&amp;paging.pageSize=10.
/// One page of dosage forms for the admin list screen; Paging comes from BasePagedQuery.
/// </summary>
public sealed class ListDosageFormsQuery : BasePagedQuery<ListDosageFormsQueryDto>
{
    /// <summary>Optional: part of the name, e.g. "tab" finds "Tableta".</summary>
    public string? Search { get; init; }
}
