using FluentValidation.Results;

namespace Market.Application.Modules.Pharmacies.Commands.Create;

/// <summary>
/// Polica (#7): creates a pharmacy. Runs after <see cref="CreatePharmacyCommandValidator"/> has accepted the format.
/// </summary>
public sealed class CreatePharmacyCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<CreatePharmacyCommand, int>
{
    public async Task<int> Handle(CreatePharmacyCommand request, CancellationToken ct)
    {
        // Defense in depth: the endpoint already has [Authorize(Policy = PolicyNames.Admin)], but the handler
        // checks the role again, so the rule holds even if the handler is ever called another way.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može dodavati apoteke.");

        // The code is always stored trimmed and in UPPERCASE. Because every stored code is uppercase, the
        // uniqueness check below can compare with == and is still case-insensitive ("ap-01" finds "AP-01"),
        // on SQL Server and on the InMemory test database alike.
        var code = request.Code.Trim().ToUpperInvariant();

        // Both database rules are checked before throwing, so the form can show both errors at once.
        var failures = new List<ValidationFailure>();

        // The global query filter skips soft-deleted rows, the same rows the filtered unique index ignores.
        if (await ctx.Pharmacies.AnyAsync(x => x.Code == code, ct))
        {
            // A FIELD error (400 on "code") instead of a 409, so the Angular form shows it under the Code field
            // (applyServerErrors); same approach as the code lists (#6).
            failures.Add(new ValidationFailure(nameof(CreatePharmacyCommand.Code),
                $"Apoteka sa šifrom \"{code}\" već postoji."));
        }

        // The foreign key would also reject an unknown CityId, but only at SaveChanges, as a DbUpdateException
        // (409 "database.error") that the form cannot place under the City field.
        if (!await ctx.Cities.AnyAsync(x => x.Id == request.CityId, ct))
        {
            failures.Add(new ValidationFailure(nameof(CreatePharmacyCommand.CityId), "Odabrani grad ne postoji."));
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        var pharmacy = new PharmacyEntity
        {
            Name = request.Name.Trim(),
            Code = code,
            Address = request.Address.Trim(),
            Phone = request.Phone.Trim(),
            CityId = request.CityId,
            IsActive = true // a new pharmacy is active; see PharmacyEntity.IsActive
        };

        // CreatedAtUtc and IsDeleted are filled in by DatabaseContext.ApplyAuditAndSoftDelete on save.
        ctx.Pharmacies.Add(pharmacy);
        await ctx.SaveChangesAsync(ct);

        // The database generated the Id (IDENTITY column) during SaveChangesAsync.
        return pharmacy.Id;
    }
}
