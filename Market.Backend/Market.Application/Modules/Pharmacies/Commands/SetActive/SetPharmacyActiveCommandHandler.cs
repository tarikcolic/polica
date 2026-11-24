namespace Market.Application.Modules.Pharmacies.Commands.SetActive;

/// <summary>
/// Polica (#7): sets PharmacyEntity.IsActive. An inactive pharmacy keeps all its data, but the lookup
/// (GetPharmaciesLookupQuery) no longer offers it in dropdowns.
/// </summary>
public sealed class SetPharmacyActiveCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<SetPharmacyActiveCommand, Unit>
{
    public async Task<Unit> Handle(SetPharmacyActiveCommand request, CancellationToken ct)
    {
        // Defense in depth, as in CreatePharmacyCommandHandler.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može aktivirati i deaktivirati apoteke.");

        var pharmacy = await ctx.Pharmacies
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Apoteka (ID={request.Id}) nije pronađena.");

        // Already in the requested state (e.g. a double click, or two admins at once) → nothing to do.
        // Not an error: the result the caller wanted is already true.
        if (pharmacy.IsActive == request.IsActive)
            return Unit.Value;

        // Deferred rule from issue #7, added in #13 together with StockItem: when request.IsActive is false,
        // deactivation is blocked while the pharmacy holds stock (any StockItem with Quantity > 0), with a
        // MarketBusinessRuleException (409). Until StockItem exists there is nothing to check.

        pharmacy.IsActive = request.IsActive;

        // ModifiedAtUtc is set by DatabaseContext.ApplyAuditAndSoftDelete.
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
