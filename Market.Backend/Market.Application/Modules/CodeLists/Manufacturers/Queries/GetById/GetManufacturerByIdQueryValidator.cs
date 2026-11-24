namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;

/// <summary>
/// Polica (#6): rejects a non-positive Id with 400 before the database is queried.
/// </summary>
public sealed class GetManufacturerByIdQueryValidator : AbstractValidator<GetManufacturerByIdQuery>
{
    public GetManufacturerByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");
    }
}
