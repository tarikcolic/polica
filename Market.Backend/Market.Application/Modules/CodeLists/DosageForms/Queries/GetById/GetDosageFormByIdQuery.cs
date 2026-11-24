namespace Market.Application.Modules.CodeLists.DosageForms.Queries.GetById;

/// <summary>
/// Polica (#6): GET api/dosage-forms/{id}. Loads one dosage form, e.g. to fill the edit dialog.
/// </summary>
public sealed class GetDosageFormByIdQuery : IRequest<GetDosageFormByIdQueryDto>
{
    public required int Id { get; init; }
}
