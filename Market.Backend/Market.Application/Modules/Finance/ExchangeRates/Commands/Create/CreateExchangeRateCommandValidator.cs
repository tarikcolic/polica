using Market.Domain.Entities.Finance;

namespace Market.Application.Modules.Finance.ExchangeRates.Commands.Create;

public sealed class CreateExchangeRateCommandValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateCommandValidator()
    {
        // Only the request itself is checked here (no database access, see ValidationBehavior).
        // Whether the currency exists and is not the base currency is checked by the handler (404 / 409).
        // Cascade.Stop: an empty code is reported only as "obavezna", not also as a wrong length.
        RuleFor(x => x.CurrencyCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Valuta je obavezna.")
            .Length(CurrencyEntity.Constraints.CodeLength).WithMessage("Oznaka valute mora imati tačno 3 slova (npr. USD).");

        RuleFor(x => x.RateToBase)
            .GreaterThan(0).WithMessage("Kurs mora biti veći od 0.")
            .PrecisionScale(ExchangeRateEntity.Constraints.RatePrecision, ExchangeRateEntity.Constraints.RateScale, ignoreTrailingZeros: true)
                .WithMessage($"Kurs može imati najviše {ExchangeRateEntity.Constraints.RateScale} decimala.");

        RuleFor(x => x.ValidFromUtc)
            .NotEmpty().WithMessage("Datum od kada kurs važi je obavezan.");
    }
}
