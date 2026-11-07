namespace Market.Application.Modules.Catalog.Products.Commands.Update;

public sealed class UpdateProductCommandHandler(IAppDbContext ctx)
            : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        // Input format (required name, lengths, price, stock) is checked by UpdateProductCommandValidator.
        // Rules that need the database are checked here (see ValidationBehavior):
        // missing product → 404, duplicate name → 409, missing category → 404, disabled category → 409.
        var entity = await ctx.Products
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Proizvod (ID={request.Id}) nije pronađen.");

        // Case-insensitive and the product itself is skipped; soft-deleted products are skipped by the global
        // query filter. The unique index stays as the last guard (DbUpdateException → 409).
        var name = request.Name.Trim();
        var normalized = name.ToLower();
        if (await ctx.Products.AnyAsync(p => p.Id != request.Id && p.Name.ToLower() == normalized, ct))
            throw new MarketConflictException("Proizvod sa ovim nazivom već postoji.");

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

        entity.Name = name;
        entity.Description = request.Description?.Trim();
        entity.Price = request.Price;
        entity.StockQuantity = request.StockQuantity;
        entity.CategoryId = request.CategoryId;
        entity.IsEnabled = request.IsEnabled;

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
