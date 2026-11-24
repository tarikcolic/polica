namespace Market.Application.Modules.Pharmacies.Queries.Lookup;

/// <summary>
/// Polica (#7): GET api/pharmacies/lookup. The ACTIVE pharmacies as (Id, Code, Name), sorted by code, for dropdowns.
/// </summary>
/// <remarks>
/// Business rule of #7: an inactive pharmacy is not offered in dropdowns. Every dropdown that picks a pharmacy
/// (employee assignment in #8, transfer destination in #18) uses this one query, so the rule is in one place.
/// Not paged: a chain has tens of pharmacies, and a dropdown needs them all at once. Open to every logged-in
/// user (a manager picks a transfer destination in #18).
/// </remarks>
public sealed class GetPharmaciesLookupQuery : IRequest<List<PharmacyLookupQueryDto>>
{
}
