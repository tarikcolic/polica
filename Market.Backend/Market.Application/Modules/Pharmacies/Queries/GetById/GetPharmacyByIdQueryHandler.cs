namespace Market.Application.Modules.Pharmacies.Queries.GetById;

/// <summary>
/// Polica (#7): returns one pharmacy as a DTO, or 404 when it does not exist.
/// </summary>
/// <remarks>
/// Authorization lives HERE, not on the endpoint: the endpoint is open to every logged-in user, because from #8 on
/// a manager or pharmacist may read their OWN pharmacy. That rule needs the user's PharmacyId, which only the
/// handler can compare with the requested Id.
/// </remarks>
public sealed class GetPharmacyByIdQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<GetPharmacyByIdQuery, GetPharmacyByIdQueryDto>
{
    public async Task<GetPharmacyByIdQueryDto> Handle(GetPharmacyByIdQuery request, CancellationToken ct)
    {
        // #7: only the administrator. #8 relaxes this to "administrator, or a user whose PharmacyId == request.Id"
        // (PharmacyId read from the database per request, not from the JWT), so a manager asking for another
        // pharmacy still gets 403.
        if (!currentUser.IsAdmin)
            throw new MarketForbiddenException("Nemate pristup ovoj apoteci.");

        // Projection straight into the DTO; x.City.Name is a JOIN in the same SQL query.
        var pharmacy = await ctx.Pharmacies
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetPharmacyByIdQueryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                CityId = x.CityId,
                CityName = x.City.Name,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);

        // MarketExceptionHandler maps this exception to 404 "entity.error".
        return pharmacy ?? throw new MarketNotFoundException($"Apoteka (ID={request.Id}) nije pronađena.");
    }
}
