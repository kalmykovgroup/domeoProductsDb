using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly ProductsDbContext _db;

    public BrandRepository(ProductsDbContext db) => _db = db;

    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken ct) =>
        await _db.Brands.AsNoTracking().OrderBy(b => b.TitleRu).ToListAsync(ct);
}
