namespace Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Update;

/// <summary>
/// Polica (#6): format rules for <see cref="UpdateWriteOffReasonCommand"/>; uniqueness is checked in the handler.
/// </summary>
public sealed class UpdateWriteOffReasonCommandValidator : AbstractValidator<UpdateWriteOffReasonCommand>
{
    public UpdateWriteOffReasonCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(WriteOffReasonEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {WriteOffReasonEntity.Constraints.NameMaxLength} znakova.");
    }
}
