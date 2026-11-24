namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Update;

/// <summary>
/// Polica (#6): request body of PUT api/dosage-forms/{id}. Renames a dosage form.
/// </summary>
public sealed class UpdateDosageFormCommand : IRequest<Unit>
{
    /// <summary>From the route ({id}); [JsonIgnore] keeps it out of the JSON body.</summary>
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>Required; max 50 characters; unique among the other dosage forms.</summary>
    public required string Name { get; set; }
}
