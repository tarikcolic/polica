namespace Market.Application.Modules.Pharmacies.Commands.SetActive;

/// <summary>
/// Polica (#7): rejects a non-positive Id with 400 before the database is queried.
/// </summary>
public sealed class SetPharmacyActiveCommandValidator : AbstractValidator<SetPharmacyActiveCommand>
{
    public SetPharmacyActiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");
    }
}
