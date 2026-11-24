namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.Lookup;

/// <summary>
/// Polica (#6): GET api/manufacturers/lookup. Every manufacturer as (Id, Name), sorted by name, for a
/// dropdown. The first user is the medication form in #10 (pick the manufacturer of a medication).
/// </summary>
/// <remarks>
/// Not paged on purpose: a dropdown needs all options at once, and a code list is small (tens of rows).
/// The admin list screen uses the paged ListManufacturersQuery instead.
/// </remarks>
public sealed class GetManufacturersLookupQuery : IRequest<List<ManufacturerLookupQueryDto>>
{
}
