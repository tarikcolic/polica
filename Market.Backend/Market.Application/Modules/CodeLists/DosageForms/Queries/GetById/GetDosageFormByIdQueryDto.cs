namespace Market.Application.Modules.CodeLists.DosageForms.Queries.GetById;

/// <summary>
/// Polica (#6): one dosage form for the edit dialog.
/// </summary>
public sealed class GetDosageFormByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
