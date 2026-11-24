namespace Market.Application.Modules.Pharmacies.Commands.Create;

/// <summary>
/// Polica (#7): request body of POST api/pharmacies. Creates a pharmacy and returns its new Id.
/// </summary>
/// <remarks>
/// CQRS: a command changes data. MediatR first runs <see cref="CreatePharmacyCommandValidator"/>
/// (ValidationBehavior: format rules) and then <see cref="CreatePharmacyCommandHandler"/> (rules that need the
/// database: unique code, existing city).
/// There is no IsActive here: a new pharmacy is always active, and only the activate/deactivate endpoints
/// change it (see PharmacyEntity.IsActive).
/// </remarks>
public sealed class CreatePharmacyCommand : IRequest<int>
{
    /// <summary>Required; max 100.</summary>
    public required string Name { get; set; }

    /// <summary>Required; max 10; letters A-Z, digits and "-". Stored in uppercase; unique (handler).</summary>
    public required string Code { get; set; }

    /// <summary>Required; max 200.</summary>
    public required string Address { get; set; }

    /// <summary>Required; max 30; digits, spaces and + - / ( ).</summary>
    public required string Phone { get; set; }

    /// <summary>Required; must be an existing city (handler).</summary>
    public required int CityId { get; set; }
}
