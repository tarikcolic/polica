namespace Market.Application.Modules.CodeLists.DosageForms.Queries.Lookup;

/// <summary>
/// Polica (#6): GET api/dosage-forms/lookup. Every dosage form as (Id, Name), sorted by name, for a
/// dropdown: the medication form and the catalog filter in #10. Not paged (a dropdown needs all options).
/// </summary>
public sealed class GetDosageFormsLookupQuery : IRequest<List<DosageFormLookupQueryDto>>
{
}
