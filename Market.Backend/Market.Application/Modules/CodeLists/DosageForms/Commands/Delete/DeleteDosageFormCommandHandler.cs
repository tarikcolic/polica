namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Delete;

/// <summary>
/// Polica (#6): deletes a dosage form (soft delete, same as DeleteManufacturerCommandHandler).
/// </summary>
public sealed class DeleteDosageFormCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<DeleteDosageFormCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDosageFormCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može brisati oblike lijeka.");

        var dosageForm = await ctx.DosageForms
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Oblik lijeka (ID={request.Id}) nije pronađen.");

        // In-use check: added in #10 together with Medication.DosageFormId. A dosage form that a medication
        // uses must not be deleted; checked in code because a soft delete never reaches the FK (Restrict).

        // Marked Deleted → saved as IsDeleted = 1 by DatabaseContext.ApplyAuditAndSoftDelete.
        ctx.DosageForms.Remove(dosageForm);
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
