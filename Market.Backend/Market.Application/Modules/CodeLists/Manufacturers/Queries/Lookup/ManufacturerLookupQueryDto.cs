namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.Lookup;

/// <summary>
/// Polica (#6): one dropdown option: the value (Id) and the text shown to the user (Name).
/// </summary>
public sealed class ManufacturerLookupQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
