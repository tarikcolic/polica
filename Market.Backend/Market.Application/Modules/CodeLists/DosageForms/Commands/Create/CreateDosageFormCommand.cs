namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Create;

/// <summary>
/// Polica (#6): request body of POST api/dosage-forms. Creates a dosage form and returns its new Id.
/// Same flow as the manufacturers: controller → MediatR → validator → handler.
/// </summary>
public sealed class CreateDosageFormCommand : IRequest<int>
{
    /// <summary>Required; max 50 characters; unique (checked in the handler).</summary>
    public required string Name { get; set; }
}
