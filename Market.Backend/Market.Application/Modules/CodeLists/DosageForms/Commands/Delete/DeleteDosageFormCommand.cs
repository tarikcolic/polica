namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Delete;

/// <summary>
/// Polica (#6): DELETE api/dosage-forms/{id}. No body; the controller builds the command from the route.
/// </summary>
public sealed class DeleteDosageFormCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
