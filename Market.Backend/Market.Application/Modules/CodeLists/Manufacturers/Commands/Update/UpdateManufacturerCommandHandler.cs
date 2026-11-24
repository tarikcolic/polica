using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Update;

/// <summary>
/// Polica (#6): updates a manufacturer's name and country.
/// </summary>
public sealed class UpdateManufacturerCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateManufacturerCommand, Unit>
{
    public async Task<Unit> Handle(UpdateManufacturerCommand request, CancellationToken ct)
    {
        // Defense in depth, as in CreateManufacturerCommandHandler.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može mijenjati proizvođače.");

        // TRACKING query (no AsNoTracking): EF remembers the loaded values, so after the properties are
        // changed below, SaveChangesAsync writes an UPDATE with only the changed columns.
        var manufacturer = await ctx.Manufacturers
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Proizvođač (ID={request.Id}) nije pronađen.");

        var name = request.Name.Trim();
        var nameLower = name.ToLower();

        // Same uniqueness rule as on create, but the record being edited is excluded (x.Id != request.Id),
        // otherwise saving without changing the name, or changing only its case, would report a duplicate.
        var nameTaken = await ctx.Manufacturers
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            // Field error on "name" (400), shown under the field in the dialog; see the create handler.
            throw new ValidationException(
                [new ValidationFailure(nameof(UpdateManufacturerCommand.Name), $"Proizvođač sa nazivom \"{name}\" već postoji.")]);
        }

        manufacturer.Name = name;
        manufacturer.Country = request.Country.TrimToNull();

        // ModifiedAtUtc is set by DatabaseContext.ApplyAuditAndSoftDelete.
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
