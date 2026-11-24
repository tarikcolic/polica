namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Create;

/// <summary>
/// Polica (#6): format rules for <see cref="CreateDosageFormCommand"/> (400 with a field error on "name").
/// No database access here; uniqueness is checked in the handler (project rule, see ValidationBehavior).
/// </summary>
public sealed class CreateDosageFormCommandValidator : AbstractValidator<CreateDosageFormCommand>
{
    public CreateDosageFormCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(DosageFormEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {DosageFormEntity.Constraints.NameMaxLength} znakova.");
    }
}
