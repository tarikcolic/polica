namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Update;

/// <summary>
/// Polica (#6): request body of PUT api/write-off-reasons/{id}. Replaces the name and the RequiresNote flag.
/// </summary>
public sealed class UpdateWriteOffReasonCommand : IRequest<Unit>
{
    /// <summary>From the route ({id}); [JsonIgnore] keeps it out of the JSON body.</summary>
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>Required; max 100 characters; unique among the other reasons.</summary>
    public required string Name { get; set; }

    /// <summary>
    /// Required in the JSON (the "required" keyword): PUT replaces the whole record, so a client that forgets
    /// the flag gets a 400 instead of silently switching the note requirement off.
    /// </summary>
    public required bool RequiresNote { get; set; }
}
