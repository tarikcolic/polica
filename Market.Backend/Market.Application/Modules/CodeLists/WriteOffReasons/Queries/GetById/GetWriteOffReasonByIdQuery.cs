namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.GetById;

/// <summary>
/// Polica (#6): GET api/write-off-reasons/{id}. Loads one write-off reason, e.g. to fill the edit dialog.
/// </summary>
public sealed class GetWriteOffReasonByIdQuery : IRequest<GetWriteOffReasonByIdQueryDto>
{
    public required int Id { get; init; }
}
