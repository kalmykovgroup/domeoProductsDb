using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _db;

    public ProductRepository(ProductsDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        int? categoryId, string? query, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<Product> q = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Offers).ThenInclude(o => o.Supplier);

        if (categoryId is not null)
            q = q.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var like = $"%{query.Trim()}%";
            q = q.Where(p => EF.Functions.ILike(p.NameRu, like)
                          || EF.Functions.ILike(p.ExternalCode, like));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task<Product?> GetDetailAsync(int id, CancellationToken ct) =>
        _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Offers).ThenInclude(o => o.Supplier)
            .Include(p => p.Attributes).ThenInclude(a => a.Attribute)
            .Include(p => p.Attributes).ThenInclude(a => a.EnumValue)
            .Include(p => p.Attributes).ThenInclude(a => a.Brand)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
