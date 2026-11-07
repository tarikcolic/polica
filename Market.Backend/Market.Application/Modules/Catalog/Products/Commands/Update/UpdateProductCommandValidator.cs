namespace Market.Application.Modules.Catalog.Products.Commands.Update;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        // Only the request itself is checked here (no database access, see ValidationBehavior).
        // The unique name needs the database, so it is checked by the handler (409).
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(ProductEntity.Constraints.NameMaxLength).WithMessage($"Naziv može imati najviše {ProductEntity.Constraints.NameMaxLength} znakova.");

        RuleFor(x => x.Description)
            .MaximumLength(ProductEntity.Constraints.DescriptionMaxLength).WithMessage($"Opis može imati najviše {ProductEntity.Constraints.DescriptionMaxLength} znakova.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Cijena ne može biti negativna.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Količina na stanju ne može biti negativna.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kategorija je obavezna.");
    }
}
