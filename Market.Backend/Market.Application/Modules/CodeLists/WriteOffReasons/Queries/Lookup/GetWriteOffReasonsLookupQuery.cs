namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.Lookup;

/// <summary>
/// Polica (#6): GET api/write-off-reasons/lookup. Every reason as (Id, Name, RequiresNote), sorted by name,
/// for the dropdown in the write-off proposal (#20). Not paged (a dropdown needs all options).
/// </summary>
public sealed class GetWriteOffReasonsLookupQuery : IRequest<List<WriteOffReasonLookupQueryDto>>
{
}
