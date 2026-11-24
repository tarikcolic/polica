namespace Market.Application.Modules.Pharmacies.Queries.GetById;

/// <summary>
/// Polica (#7): GET api/pharmacies/{id}. Loads one pharmacy, e.g. to fill the edit page.
/// </summary>
public sealed class GetPharmacyByIdQuery : IRequest<GetPharmacyByIdQueryDto>
{
    public required int Id { get; init; }
}
