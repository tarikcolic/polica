namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Create;

/// <summary>
/// Polica (#6): format rules for <see cref="CreateWriteOffReasonCommand"/> (400 with a field error on "name").
/// RequiresNote is a bool, so any value is valid and it needs no rule. Uniqueness is checked in the handler.
/// </summary>
public sealed class CreateWriteOffReasonCommandValidator : AbstractValidator<CreateWriteOffReasonCommand>
{
    public CreateWriteOffReasonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(WriteOffReasonEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {WriteOffReasonEntity.Constraints.NameMaxLength} znakova.");
    }
}
