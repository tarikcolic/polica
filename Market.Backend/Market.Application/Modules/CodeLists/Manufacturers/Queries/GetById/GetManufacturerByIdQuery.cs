namespace Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;

/// <summary>
/// Polica (#6): GET api/manufacturers/{id}. Loads one manufacturer, e.g. to fill the edit dialog.
/// </summary>
public sealed class GetManufacturerByIdQuery : IRequest<GetManufacturerByIdQueryDto>
{
    public required int Id { get; init; }
}
