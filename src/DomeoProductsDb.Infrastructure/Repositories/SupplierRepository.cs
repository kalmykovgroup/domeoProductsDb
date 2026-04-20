using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly ProductsDbContext _db;

    public SupplierRepository(ProductsDbContext db) => _db = db;

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct) =>
        await _db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);

    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
}
