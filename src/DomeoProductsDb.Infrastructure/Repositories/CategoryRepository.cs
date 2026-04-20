using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductsDbContext _db;

    public CategoryRepository(ProductsDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct) =>
        await _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetByCodeAsync(string code, CancellationToken ct) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, ct);
}
