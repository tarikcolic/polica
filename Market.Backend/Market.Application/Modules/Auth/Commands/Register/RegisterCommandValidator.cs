namespace Market.Application.Modules.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Firstname)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(MarketUserEntity.Constraints.FirstnameMaxLength)
            .WithMessage($"Ime može imati najviše {MarketUserEntity.Constraints.FirstnameMaxLength} znakova.");

        RuleFor(x => x.Lastname)
            .NotEmpty().WithMessage("Prezime je obavezno.")
            .MaximumLength(MarketUserEntity.Constraints.LastnameMaxLength)
            .WithMessage($"Prezime može imati najviše {MarketUserEntity.Constraints.LastnameMaxLength} znakova.");

        // Only the request itself is checked here (no database access, see ValidationBehavior).
        // An email that is already registered needs the database, so it is checked by RegisterCommandHandler (409).
        // Cascade.Stop: an empty email is reported only as "obavezan", not also as "nije ispravan".
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email je obavezan.")
            .MaximumLength(MarketUserEntity.Constraints.EmailMaxLength)
            .WithMessage($"Email može imati najviše {MarketUserEntity.Constraints.EmailMaxLength} znakova.")
            .EmailAddress().WithMessage("Email nije ispravan.");

        RuleFor(x => x.Password)
            .ValidPassword();

        RuleFor(x => x.Address)
            .MaximumLength(MarketUserEntity.Constraints.AddressMaxLength)
            .WithMessage($"Adresa može imati najviše {MarketUserEntity.Constraints.AddressMaxLength} znakova.");

        RuleFor(x => x.City)
            .MaximumLength(MarketUserEntity.Constraints.CityMaxLength)
            .WithMessage($"Grad može imati najviše {MarketUserEntity.Constraints.CityMaxLength} znakova.");
    }
}
