namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;

/// <summary>
/// Polica (#6): returns one manufacturer as a DTO, or 404 when it does not exist (or was deleted).
/// </summary>
public sealed class GetManufacturerByIdQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetManufacturerByIdQuery, GetManufacturerByIdQueryDto>
{
    public async Task<GetManufacturerByIdQueryDto> Handle(GetManufacturerByIdQuery request, CancellationToken ct)
    {
        // Projection straight into the DTO; a soft-deleted row is hidden by the global filter → null → 404.
        var manufacturer = await ctx.Manufacturers
            .Where(x => x.Id == request.Id)
            .Select(x => new GetManufacturerByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                Country = x.Country
            })
            .FirstOrDefaultAsync(ct);

        // MarketExceptionHandler maps this exception to 404 "entity.error".
        return manufacturer ?? throw new MarketNotFoundException($"Proizvođač (ID={request.Id}) nije pronađen.");
    }
}
