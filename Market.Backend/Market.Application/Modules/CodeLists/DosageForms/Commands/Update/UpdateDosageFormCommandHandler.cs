using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Update;

/// <summary>
/// Polica (#6): renames a dosage form (same steps as UpdateManufacturerCommandHandler).
/// </summary>
public sealed class UpdateDosageFormCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateDosageFormCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDosageFormCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može mijenjati oblike lijeka.");

        // Tracking query: the change below is saved as an UPDATE.
        var dosageForm = await ctx.DosageForms
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Oblik lijeka (ID={request.Id}) nije pronađen.");

        var name = request.Name.Trim();
        var nameLower = name.ToLower();

        // The edited row itself is excluded, so changing only the letter case is allowed.
        var nameTaken = await ctx.DosageForms
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(UpdateDosageFormCommand.Name), $"Oblik lijeka sa nazivom \"{name}\" već postoji.")]);
        }

        dosageForm.Name = name;
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
