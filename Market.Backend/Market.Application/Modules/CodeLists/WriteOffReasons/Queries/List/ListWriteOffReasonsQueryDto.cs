namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.List;

/// <summary>
/// Polica (#6): one row of the write-off reasons table (DTO, never the entity).
/// </summary>
public sealed class ListWriteOffReasonsQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }

    /// <summary>Shown as a badge in the list ("Napomena obavezna").</summary>
    public required bool RequiresNote { get; init; }
}
