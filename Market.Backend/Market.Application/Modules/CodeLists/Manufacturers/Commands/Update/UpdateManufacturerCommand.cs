namespace Market.Application.Modules.CodeLists.Manufacturers.Commands.Update;

/// <summary>
/// Polica (#6): request body of PUT api/manufacturers/{id}. Replaces the name and country.
/// </summary>
public sealed class UpdateManufacturerCommand : IRequest<Unit>
{
    /// <summary>
    /// Taken from the route ({id}), not from the body: [JsonIgnore] hides it from the JSON and Swagger,
    /// and the controller sets it (command.Id = id) before sending the command.
    /// </summary>
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>Required; max 100 characters; unique among the other manufacturers.</summary>
    public required string Name { get; set; }

    /// <summary>Optional; max 100 characters. Sending null or "" clears it.</summary>
    public string? Country { get; set; }
}
