using FluentValidation.Results;

namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Create;

/// <summary>
/// Polica (#6): creates a manufacturer. Runs after <see cref="CreateManufacturerCommandValidator"/> has
/// accepted the format (required name, max lengths).
/// </summary>
public sealed class CreateManufacturerCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<CreateManufacturerCommand, int>
{
    public async Task<int> Handle(CreateManufacturerCommand request, CancellationToken ct)
    {
        // Defense in depth: the endpoint already has [Authorize(Policy = PolicyNames.Admin)], but the
        // handler checks the role again, so the rule holds even if the handler is ever called another way.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može dodavati proizvođače.");

        // Stored without leading/trailing spaces; an empty country becomes null (TrimToNull).
        var name = request.Name.Trim();
        var country = request.Country.TrimToNull();

        // Unique name, case-insensitive ("pliva" = "Pliva"). ToLower() on both sides makes the comparison
        // case-insensitive in every database (the InMemory test database compares case-sensitively).
        // The global query filter skips soft-deleted rows, so a deleted name can be used again.
        var nameLower = name.ToLower();
        var nameTaken = await ctx.Manufacturers
            .AnyAsync(x => x.Name.ToLower() == nameLower, ct);

        if (nameTaken)
        {
            // Reported as a FIELD error (400, field "name") rather than a 409, so the Angular dialog shows it
            // under the Name field (applyServerErrors). Same approach as ChangePasswordCommandHandler:
            // the check needs the database, so it is done here and not in the validator.
            throw new ValidationException(
                [new ValidationFailure(nameof(CreateManufacturerCommand.Name), $"Proizvođač sa nazivom \"{name}\" već postoji.")]);
        }

        var manufacturer = new ManufacturerEntity
        {
            Name = name,
            Country = country
        };

        // CreatedAtUtc and IsDeleted are filled in by DatabaseContext.ApplyAuditAndSoftDelete on save.
        ctx.Manufacturers.Add(manufacturer);
        await ctx.SaveChangesAsync(ct);

        // After SaveChangesAsync the database has generated the Id (IDENTITY column).
        return manufacturer.Id;
    }
}
