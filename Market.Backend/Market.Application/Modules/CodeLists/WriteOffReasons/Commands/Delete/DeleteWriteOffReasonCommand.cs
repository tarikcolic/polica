namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Delete;

/// <summary>
/// Polica (#6): DELETE api/write-off-reasons/{id}. No body; the controller builds the command from the route.
/// </summary>
public sealed class DeleteWriteOffReasonCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
