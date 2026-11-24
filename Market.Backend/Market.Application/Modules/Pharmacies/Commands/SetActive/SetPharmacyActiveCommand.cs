namespace Market.Application.Modules.Pharmacies.Commands.SetActive;

/// <summary>
/// Polica (#7): activates or deactivates a pharmacy. Sent by two endpoints without a body:
/// POST api/pharmacies/{id}/activate (IsActive = true) and POST api/pharmacies/{id}/deactivate (IsActive = false).
/// </summary>
/// <remarks>
/// One command with a flag instead of two commands: both do the same thing (load, set IsActive, save). The rule
/// that applies only to deactivation (no stock, #13) is a single "if (!request.IsActive)" in the handler.
/// Two separate URLs still keep the API readable in Swagger and in the browser's network tab.
/// </remarks>
public sealed class SetPharmacyActiveCommand : IRequest<Unit>
{
    public required int Id { get; init; }

    /// <summary>true = activate, false = deactivate.</summary>
    public required bool IsActive { get; init; }
}
