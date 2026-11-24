namespace Market.Application.Modules.Pharmacies.Commands.Create;

/// <summary>
/// Polica (#7): format rules for <see cref="CreatePharmacyCommand"/>, run by ValidationBehavior before the handler.
/// A failure → 400 "validation.error" with a field error per property ("name", "code", ...), which the Angular
/// form shows under the matching field.
/// </summary>
/// <remarks>
/// Project rule (see ValidationBehavior): a validator never reads the database, so "the code must be unique"
/// and "the city must exist" are checked in the handler. The limits come from PharmacyEntity.Constraints,
/// the same numbers the EF configuration uses for the column sizes.
/// </remarks>
public sealed class CreatePharmacyCommandValidator : AbstractValidator<CreatePharmacyCommand>
{
    /// <summary>
    /// Letters A-Z (either case: the handler uppercases them), digits and "-", e.g. "AP-01".
    /// No spaces or diacritics, so the code is easy to type and to print on documents.
    /// </summary>
    public const string CodePattern = "^[A-Za-z0-9-]+$";

    /// <summary>Digits, spaces and the characters + - / ( ), e.g. "+387 33 123 456" or "033/123-456".</summary>
    public const string PhonePattern = @"^[0-9+()/ -]+$";

    public CreatePharmacyCommandValidator()
    {
        // NotEmpty also rejects a value made only of spaces.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(PharmacyEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {PharmacyEntity.Constraints.NameMaxLength} znakova.");

        // Lowercase letters are accepted here; the handler stores the code in uppercase ("ap-01" → "AP-01").
        // A space anywhere (also at the ends) fails the pattern; the Angular form trims before sending.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Šifra je obavezna.")
            .MaximumLength(PharmacyEntity.Constraints.CodeMaxLength)
            .WithMessage($"Šifra može imati najviše {PharmacyEntity.Constraints.CodeMaxLength} znakova.")
            .Matches(CodePattern)
            .WithMessage("Šifra može sadržavati samo slova A-Z, cifre i crticu (npr. AP-01).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adresa je obavezna.")
            .MaximumLength(PharmacyEntity.Constraints.AddressMaxLength)
            .WithMessage($"Adresa može imati najviše {PharmacyEntity.Constraints.AddressMaxLength} znakova.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon je obavezan.")
            .MaximumLength(PharmacyEntity.Constraints.PhoneMaxLength)
            .WithMessage($"Telefon može imati najviše {PharmacyEntity.Constraints.PhoneMaxLength} znakova.")
            .Matches(PhonePattern)
            .WithMessage("Telefon može sadržavati samo cifre, razmake i znakove + - / ( ).");

        // Only "a city was chosen" here; whether that city exists is a database question (handler).
        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Grad je obavezan.");
    }
}
