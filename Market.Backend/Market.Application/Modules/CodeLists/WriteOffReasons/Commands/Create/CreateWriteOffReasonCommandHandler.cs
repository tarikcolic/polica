using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Create;

/// <summary>
/// Polica (#6): creates a write-off reason. Same steps as CreateManufacturerCommandHandler:
/// role check, trim, case-insensitive uniqueness check, insert.
/// </summary>
public sealed class CreateWriteOffReasonCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<CreateWriteOffReasonCommand, int>
{
    public async Task<int> Handle(CreateWriteOffReasonCommand request, CancellationToken ct)
    {
        // Defense in depth: the endpoint already requires the "Admin" policy.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može dodavati razloge otpisa.");

        var name = request.Name.Trim();
        var nameLower = name.ToLower();

        // Case-insensitive; soft-deleted rows are ignored (global query filter).
        var nameTaken = await ctx.WriteOffReasons
            .AnyAsync(x => x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            // 400 with a field error on "name", shown under the field in the dialog.
            throw new ValidationException(
                [new ValidationFailure(nameof(CreateWriteOffReasonCommand.Name), $"Razlog otpisa sa nazivom \"{name}\" već postoji.")]);
        }

        var reason = new WriteOffReasonEntity
        {
            Name = name,
            RequiresNote = request.RequiresNote
        };

        ctx.WriteOffReasons.Add(reason);
        await ctx.SaveChangesAsync(ct);

        return reason.Id;
    }
}
