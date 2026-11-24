namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;

/// <summary>
/// Polica (#6): one manufacturer with every field the edit dialog needs.
/// </summary>
public sealed class GetManufacturerByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? Country { get; init; }
}
