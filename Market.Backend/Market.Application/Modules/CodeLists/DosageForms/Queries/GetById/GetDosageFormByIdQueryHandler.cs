namespace Market.Application.Modules.CodeLists.DosageForms.Queries.GetById;

/// <summary>
/// Polica (#6): returns one dosage form as a DTO, or 404 when it does not exist (or was deleted).
/// </summary>
public sealed class GetDosageFormByIdQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetDosageFormByIdQuery, GetDosageFormByIdQueryDto>
{
    public async Task<GetDosageFormByIdQueryDto> Handle(GetDosageFormByIdQuery request, CancellationToken ct)
    {
        var dosageForm = await ctx.DosageForms
            .Where(x => x.Id == request.Id)
            .Select(x => new GetDosageFormByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .FirstOrDefaultAsync(ct);

        return dosageForm ?? throw new MarketNotFoundException($"Oblik lijeka (ID={request.Id}) nije pronađen.");
    }
}
