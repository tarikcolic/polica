namespace Market.Application.Modules.CodeLists.WriteOffReasons.Queries.GetById;

/// <summary>
/// Polica (#6): returns one write-off reason as a DTO, or 404 when it does not exist (or was deleted).
/// </summary>
public sealed class GetWriteOffReasonByIdQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetWriteOffReasonByIdQuery, GetWriteOffReasonByIdQueryDto>
{
    public async Task<GetWriteOffReasonByIdQueryDto> Handle(GetWriteOffReasonByIdQuery request, CancellationToken ct)
    {
        var reason = await ctx.WriteOffReasons
            .Where(x => x.Id == request.Id)
            .Select(x => new GetWriteOffReasonByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                RequiresNote = x.RequiresNote
            })
            .FirstOrDefaultAsync(ct);

        return reason ?? throw new MarketNotFoundException($"Razlog otpisa (ID={request.Id}) nije pronađen.");
    }
}
