namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Update;

/// <summary>
/// Polica (#6): format rules for <see cref="UpdateManufacturerCommand"/>; the same field rules as
/// CreateManufacturerCommandValidator, plus a valid Id. Uniqueness is checked in the handler.
/// </summary>
public sealed class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturerCommand>
{
    public UpdateManufacturerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(ManufacturerEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {ManufacturerEntity.Constraints.NameMaxLength} znakova.");

        RuleFor(x => x.Country)
            .MaximumLength(ManufacturerEntity.Constraints.CountryMaxLength)
            .WithMessage($"Država može imati najviše {ManufacturerEntity.Constraints.CountryMaxLength} znakova.");
    }
}
