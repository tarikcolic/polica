namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Create;

/// <summary>
/// Polica (#6): request body of POST api/write-off-reasons. Creates a write-off reason and returns its new Id.
/// Same flow as the manufacturers: controller → MediatR → validator → handler.
/// </summary>
public sealed class CreateWriteOffReasonCommand : IRequest<int>
{
    /// <summary>Required; max 100 characters; unique (checked in the handler).</summary>
    public required string Name { get; set; }

    /// <summary>
    /// True when a write-off with this reason must have a note (enforced by the write-off in #20).
    /// Optional in the JSON: when it is left out, the reason does not require a note (false).
    /// </summary>
    public bool RequiresNote { get; set; }
}
