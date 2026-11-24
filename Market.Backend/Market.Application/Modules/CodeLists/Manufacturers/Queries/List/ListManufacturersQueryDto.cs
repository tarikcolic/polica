namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.List;

/// <summary>
/// Polica (#6): one row of the manufacturers table. A DTO, so the API never returns the entity itself
/// (no IsDeleted, no audit columns, nothing the client should not see).
/// </summary>
public sealed class ListManufacturersQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? Country { get; init; }
}
