namespace Market.Application.Modules.CodeLists.DosageForms.Queries.List;

/// <summary>
/// Polica (#6): one row of the dosage forms table (DTO, never the entity).
/// </summary>
public sealed class ListDosageFormsQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
