namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Delete;

/// <summary>
/// Polica (#6): DELETE api/manufacturers/{id}. No body; the controller builds the command from the route.
/// </summary>
public sealed class DeleteManufacturerCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
