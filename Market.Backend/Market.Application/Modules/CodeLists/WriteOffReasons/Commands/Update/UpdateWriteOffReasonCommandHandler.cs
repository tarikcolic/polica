using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Update;

/// <summary>
/// Polica (#6): updates a write-off reason (same steps as UpdateManufacturerCommandHandler).
/// </summary>
/// <remarks>
/// Changing RequiresNote affects only write-offs proposed from now on (#20); notes of existing write-offs
/// are not touched.
/// </remarks>
public sealed class UpdateWriteOffReasonCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateWriteOffReasonCommand, Unit>
{
    public async Task<Unit> Handle(UpdateWriteOffReasonCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može mijenjati razloge otpisa.");

        // Tracking query: the changes below are saved as an UPDATE.
        var reason = await ctx.WriteOffReasons
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Razlog otpisa (ID={request.Id}) nije pronađen.");

        var name = request.Name.Trim();
        var nameLower = name.ToLower();

        // The edited row itself is excluded, so changing only the letter case is allowed.
        var nameTaken = await ctx.WriteOffReasons
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(UpdateWriteOffReasonCommand.Name), $"Razlog otpisa sa nazivom \"{name}\" već postoji.")]);
        }

        reason.Name = name;
        reason.RequiresNote = request.RequiresNote;
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
