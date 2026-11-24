namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.GetById;

/// <summary>
/// Polica (#6): one write-off reason for the edit dialog.
/// </summary>
public sealed class GetWriteOffReasonByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool RequiresNote { get; init; }
}
