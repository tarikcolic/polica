using FluentValidation.Results;

namespace Market.Application.Modules.Pharmacies.Commands.Update;

/// <summary>
/// Polica (#7): updates a pharmacy's name, code, address, phone and city. IsActive is not touched here
/// (see the remark on <see cref="UpdatePharmacyCommand"/>).
/// </summary>
public sealed class UpdatePharmacyCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<UpdatePharmacyCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePharmacyCommand request, CancellationToken ct)
    {
        // Defense in depth, as in CreatePharmacyCommandHandler.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Samo administrator može mijenjati apoteke.");

        // TRACKING query (no AsNoTracking): EF remembers the loaded values, so SaveChangesAsync below writes an
        // UPDATE with only the columns that really changed.
        var pharmacy = await ctx.Pharmacies
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"Apoteka (ID={request.Id}) nije pronađena.");

        var code = request.Code.Trim().ToUpperInvariant();
        var failures = new List<ValidationFailure>();

        // Same rule as on create, but the pharmacy being edited is excluded (x.Id != request.Id); otherwise
        // saving without changing the code would report the pharmacy as a duplicate of itself.
        if (await ctx.Pharmacies.AnyAsync(x => x.Id != request.Id && x.Code == code, ct))
        {
            failures.Add(new ValidationFailure(nameof(UpdatePharmacyCommand.Code),
                $"Apoteka sa šifrom \"{code}\" već postoji."));
        }

        if (!await ctx.Cities.AnyAsync(x => x.Id == request.CityId, ct))
        {
            failures.Add(new ValidationFailure(nameof(UpdatePharmacyCommand.CityId), "Odabrani grad ne postoji."));
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        pharmacy.Name = request.Name.Trim();
        pharmacy.Code = code;
        pharmacy.Address = request.Address.Trim();
        pharmacy.Phone = request.Phone.Trim();
        pharmacy.CityId = request.CityId;

        // ModifiedAtUtc is set by DatabaseContext.ApplyAuditAndSoftDelete.
        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
