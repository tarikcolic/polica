using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Create;

/// <summary>
/// Polica (#6): creates a dosage form. The steps are the same as in CreateManufacturerCommandHandler:
/// role check, trim, case-insensitive uniqueness check, insert.
/// </summary>
public sealed class CreateDosageFormCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<CreateDosageFormCommand, int>
{
    public async Task<int> Handle(CreateDosageFormCommand request, CancellationToken ct)
    {
        // Defense in depth: the endpoint already requires the "Admin" policy.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može dodavati oblike lijeka.");

        var name = request.Name.Trim();
        var nameLower = name.ToLower();

        // Case-insensitive; soft-deleted rows are ignored (global query filter).
        var nameTaken = await ctx.DosageForms
            .AnyAsync(x => x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            // 400 with a field error on "name", shown under the field in the dialog.
            throw new ValidationException(
                [new ValidationFailure(nameof(CreateDosageFormCommand.Name), $"Oblik lijeka sa nazivom \"{name}\" već postoji.")]);
        }

        var dosageForm = new DosageFormEntity { Name = name };

        ctx.DosageForms.Add(dosageForm);
        await ctx.SaveChangesAsync(ct);

        return dosageForm.Id;
    }
}
