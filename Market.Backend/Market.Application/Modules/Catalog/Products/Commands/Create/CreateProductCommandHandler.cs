namespace Market.Application.Modules.Catalog.Products.Commands.Create;

public class CreateProductCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Input format (required name, lengths, price, stock) is checked by CreateProductCommandValidator.
        // Rules that need the database are checked here (see ValidationBehavior):
        // missing category → 404, disabled category → 409, duplicate name → 409.
        var productCategory = await ctx.ProductCategories
            .Where(x => x.Id == request.CategoryId)
            .FirstOrDefaultAsync(ct);

        if (productCategory is null)
        {
            throw new MarketNotFoundException($"Kategorija (ID={request.CategoryId}) nije pronađena.");
        }

        if (!productCategory.IsEnabled)
        {
            throw new MarketBusinessRuleException("product.category.disabled",
                $"Kategorija {productCategory.Name} je onemogućena i ne može se dodijeliti proizvodu.");
        }

        // Case-insensitive, like the unique index (default SQL Server collation); soft-deleted products
        // are skipped by the global query filter. The unique index stays as the last guard (DbUpdateException → 409).
        var name = request.Name.Trim();
        var normalized = name.ToLower();
        if (await ctx.Products.AnyAsync(p => p.Name.ToLower() == normalized, ct))
        {
            throw new MarketConflictException("Proizvod sa ovim nazivom već postoji.");
        }

        var product = new ProductEntity
        {
            Name = name,
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsEnabled = request.IsEnabled
        };

        ctx.Products.Add(product);
        await ctx.SaveChangesAsync(ct);

        return product.Id;
    }
}
