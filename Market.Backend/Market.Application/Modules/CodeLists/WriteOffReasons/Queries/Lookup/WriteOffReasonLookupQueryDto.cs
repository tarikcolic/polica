namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.Lookup;

/// <summary>
/// Polica (#6): one dropdown option. Unlike the other lookups it also carries RequiresNote, so the
/// write-off form (#20) can make the note field required as soon as such a reason is picked.
/// </summary>
public sealed class WriteOffReasonLookupQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool RequiresNote { get; init; }
}
