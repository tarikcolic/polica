namespace Market.Application.Modules.CodeLists.DosageForms.Queries.GetById;

/// <summary>
/// Polica (#6): rejects a non-positive Id with 400 before the database is queried.
/// </summary>
public sealed class GetDosageFormByIdQueryValidator : AbstractValidator<GetDosageFormByIdQuery>
{
    public GetDosageFormByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");
    }
}
