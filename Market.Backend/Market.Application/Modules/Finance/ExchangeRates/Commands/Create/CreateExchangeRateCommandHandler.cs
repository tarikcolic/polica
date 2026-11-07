using Market.Domain.Entities.Finance;

namespace Market.Application.Modules.Finance.ExchangeRates.Commands.Create;

public sealed class CreateExchangeRateCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CreateExchangeRateCommand, int>
{
    /// <summary>
    /// Value of <see cref="ExchangeRateEntity.Source"/> for rates entered by the administrator.
    /// </summary>
    private const string ManualSource = "Ručni unos";

    public async Task<int> Handle(CreateExchangeRateCommand request, CancellationToken ct)
    {
        // Code format and rate > 0 are checked by CreateExchangeRateCommandValidator.
        // Rules that need the database are checked here (see ValidationBehavior):
        // unknown currency → 404, base currency → 409, rate for the same moment already exists → 409.
        var code = request.CurrencyCode.Trim().ToUpperInvariant();
        var validFromUtc = request.ValidFromUtc.AsUtc();

        var currency = await ctx.Currencies
            .FirstOrDefaultAsync(c => c.Code == code, ct);

        if (currency is null)
            throw new MarketNotFoundException($"Valuta {code} nije pronađena.");

        if (currency.IsBase)
            throw new MarketBusinessRuleException("exchangeRate.baseCurrency",
                $"Valuta {code} je bazna valuta: njen kurs je uvijek 1 i ne unosi se.");

        // One rate per currency and moment (also guarded by a unique index).
        var exists = await ctx.ExchangeRates
            .AnyAsync(r => r.CurrencyId == currency.Id && r.ValidFromUtc == validFromUtc, ct);

        if (exists)
            throw new MarketConflictException($"Za valutu {code} već postoji kurs koji važi od {validFromUtc:dd.MM.yyyy. HH:mm} (UTC).");

        var rate = new ExchangeRateEntity
        {
            CurrencyId = currency.Id,
            RateToBase = request.RateToBase,
            ValidFromUtc = validFromUtc,
            Source = ManualSource
        };

        ctx.ExchangeRates.Add(rate);
        await ctx.SaveChangesAsync(ct);

        return rate.Id;
    }
}
