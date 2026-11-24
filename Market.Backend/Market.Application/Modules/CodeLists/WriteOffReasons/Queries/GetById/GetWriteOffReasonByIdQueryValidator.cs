namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.GetById;

/// <summary>
/// Polica (#6): rejects a non-positive Id with 400 before the database is queried.
/// </summary>
public sealed class GetWriteOffReasonByIdQueryValidator : AbstractValidator<GetWriteOffReasonByIdQuery>
{
    public GetWriteOffReasonByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");
    }
}
