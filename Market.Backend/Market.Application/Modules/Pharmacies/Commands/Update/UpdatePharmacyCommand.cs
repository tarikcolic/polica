namespace Market.Application.Modules.Pharmacies.Commands.Update;

/// <summary>
/// Polica (#7): request body of PUT api/pharmacies/{id}. Replaces the pharmacy's data.
/// </summary>
/// <remarks>
/// IsActive is deliberately NOT here. Activating and deactivating have their own endpoints
/// (SetPharmacyActiveCommand), because deactivation gets its own business rule in #13 ("not while the pharmacy
/// holds stock"). If PUT could also set IsActive, that rule could be bypassed by sending isActive = false here.
/// </remarks>
public sealed class UpdatePharmacyCommand : IRequest<Unit>
{
    /// <summary>
    /// Taken from the route ({id}), not from the body: [JsonIgnore] hides it from the JSON and Swagger, and the
    /// controller sets it (command.Id = id) before sending the command.
    /// </summary>
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>Required; max 100.</summary>
    public required string Name { get; set; }

    /// <summary>Required; max 10; letters A-Z, digits and "-". Unique among the other pharmacies.</summary>
    public required string Code { get; set; }

    /// <summary>Required; max 200.</summary>
    public required string Address { get; set; }

    /// <summary>Required; max 30.</summary>
    public required string Phone { get; set; }

    /// <summary>Required; must be an existing city.</summary>
    public required int CityId { get; set; }
}
