using Market.Application.Modules.Pharmacies.Commands.Create;

namespace Market.Application.Modules.Pharmacies.Commands.Update;

/// <summary>
/// Polica (#7): format rules for <see cref="UpdatePharmacyCommand"/>: the same field rules as
/// <see cref="CreatePharmacyCommandValidator"/> (and its two patterns), plus a valid Id.
/// Uniqueness of the code and the existence of the city are checked in the handler.
/// </summary>
public sealed class UpdatePharmacyCommandValidator : AbstractValidator<UpdatePharmacyCommand>
{
    public UpdatePharmacyCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(PharmacyEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {PharmacyEntity.Constraints.NameMaxLength} znakova.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Šifra je obavezna.")
            .MaximumLength(PharmacyEntity.Constraints.CodeMaxLength)
            .WithMessage($"Šifra može imati najviše {PharmacyEntity.Constraints.CodeMaxLength} znakova.")
            .Matches(CreatePharmacyCommandValidator.CodePattern)
            .WithMessage("Šifra može sadržavati samo slova A-Z, cifre i crticu (npr. AP-01).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adresa je obavezna.")
            .MaximumLength(PharmacyEntity.Constraints.AddressMaxLength)
            .WithMessage($"Adresa može imati najviše {PharmacyEntity.Constraints.AddressMaxLength} znakova.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon je obavezan.")
            .MaximumLength(PharmacyEntity.Constraints.PhoneMaxLength)
            .WithMessage($"Telefon može imati najviše {PharmacyEntity.Constraints.PhoneMaxLength} znakova.")
            .Matches(CreatePharmacyCommandValidator.PhonePattern)
            .WithMessage("Telefon može sadržavati samo cifre, razmake i znakove + - / ( ).");

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Grad je obavezan.");
    }
}
