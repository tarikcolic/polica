namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Delete;

/// <summary>
/// Polica (#6): deletes a manufacturer (soft delete).
/// </summary>
public sealed class DeleteManufacturerCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<DeleteManufacturerCommand, Unit>
{
    public async Task<Unit> Handle(DeleteManufacturerCommand request, CancellationToken ct)
    {
        // Defense in depth, as in CreateManufacturerCommandHandler.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može brisati proizvođače.");

        var manufacturer = await ctx.Manufacturers
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Proizvođač (ID={request.Id}) nije pronađen.");

        // In-use check: added in #10, when Medication (with ManufacturerId) exists. A manufacturer that a
        // medication still uses must not be deleted; the handler will then throw a ValidationException with
        // a readable message. It has to be checked in code: a soft delete is an UPDATE (IsDeleted = 1),
        // not a DELETE, so the foreign key (Restrict) in the database would never stop it.

        // Remove() marks the entity as Deleted; DatabaseContext.ApplyAuditAndSoftDelete turns that into
        // IsDeleted = 1 + ModifiedAtUtc, so the row stays in the table and the global filter hides it.
        ctx.Manufacturers.Remove(manufacturer);
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
