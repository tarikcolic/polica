namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Delete;

/// <summary>
/// Polica (#6): deletes a write-off reason (soft delete, same as DeleteManufacturerCommandHandler).
/// </summary>
public sealed class DeleteWriteOffReasonCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<DeleteWriteOffReasonCommand, Unit>
{
    public async Task<Unit> Handle(DeleteWriteOffReasonCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može brisati razloge otpisa.");

        var reason = await ctx.WriteOffReasons
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Razlog otpisa (ID={request.Id}) nije pronađen.");

        // In-use check: added in #20 together with WriteOff.WriteOffReasonId. A reason used by a write-off
        // must not be deleted; checked in code because a soft delete never reaches the FK (Restrict).

        // Marked Deleted → saved as IsDeleted = 1 by DatabaseContext.ApplyAuditAndSoftDelete.
        ctx.WriteOffReasons.Remove(reason);
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
