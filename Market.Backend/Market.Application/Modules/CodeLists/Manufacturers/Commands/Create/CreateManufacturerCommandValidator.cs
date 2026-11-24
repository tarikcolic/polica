namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Create;

/// <summary>
/// Polica (#6): format rules for <see cref="CreateManufacturerCommand"/>, run by ValidationBehavior before
/// the handler. A failure → 400 "validation.error" with a field error per property ("name", "country"),
/// which the Angular dialog shows under the matching field.
/// </summary>
/// <remarks>
/// Project rule (see ValidationBehavior): a validator checks only the request itself and never reads the
/// database. That is why "the name must be unique" is NOT here but in the handler.
/// The limits come from ManufacturerEntity.Constraints, the same numbers the EF configuration uses.
/// </remarks>
public sealed class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
{
    public CreateManufacturerCommandValidator()
    {
        // NotEmpty also rejects a name made only of spaces.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(ManufacturerEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {ManufacturerEntity.Constraints.NameMaxLength} znakova.");

        // Country is optional: MaximumLength accepts null, so only a value that is too long fails.
        RuleFor(x => x.Country)
            .MaximumLength(ManufacturerEntity.Constraints.CountryMaxLength)
            .WithMessage($"Država može imati najviše {ManufacturerEntity.Constraints.CountryMaxLength} znakova.");
    }
}
