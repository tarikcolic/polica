namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Create;

/// <summary>
/// Polica (#6): request body of POST api/manufacturers. Creates a manufacturer and returns its new Id.
/// </summary>
/// <remarks>
/// CQRS: a command changes data. The controller only passes it to MediatR (sender.Send); MediatR first runs
/// <see cref="CreateManufacturerCommandValidator"/> (ValidationBehavior) and then
/// <see cref="CreateManufacturerCommandHandler"/>.
/// </remarks>
public sealed class CreateManufacturerCommand : IRequest<int>
{
    /// <summary>Required; max 100 characters; unique (checked in the handler).</summary>
    public required string Name { get; set; }

    /// <summary>Optional; max 100 characters. An empty value is stored as null.</summary>
    public string? Country { get; set; }
}
