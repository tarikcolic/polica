namespace Market.Application.Modules.CodeLists.DosageForms.Queries.Lookup;

/// <summary>
/// Polica (#6): one dropdown option (value = Id, text = Name).
/// </summary>
public sealed class DosageFormLookupQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
