namespace Market.Application.Modules.Pharmacies.Queries.GetById;

/// <summary>
/// Polica (#7): rejects a non-positive Id with 400 before the database is queried.
/// </summary>
public sealed class GetPharmacyByIdQueryValidator : AbstractValidator<GetPharmacyByIdQuery>
{
    public GetPharmacyByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");
    }
}
