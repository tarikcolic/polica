namespace Market.Application.Modules.CodeLists.DosageForms.Commands.Update;

/// <summary>
/// Polica (#6): format rules for <see cref="UpdateDosageFormCommand"/>; uniqueness is checked in the handler.
/// </summary>
public sealed class UpdateDosageFormCommandValidator : AbstractValidator<UpdateDosageFormCommand>
{
    public UpdateDosageFormCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivan broj.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(DosageFormEntity.Constraints.NameMaxLength)
            .WithMessage($"Naziv može imati najviše {DosageFormEntity.Constraints.NameMaxLength} znakova.");
    }
}
