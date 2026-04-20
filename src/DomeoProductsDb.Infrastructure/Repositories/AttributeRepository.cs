using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class AttributeRepository : IAttributeRepository
{
    private readonly ProductsDbContext _db;

    public AttributeRepository(ProductsDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProductAttribute>> GetAllAsync(CancellationToken ct) =>
        await _db.Attributes.AsNoTracking().OrderBy(a => a.TitleRu).ToListAsync(ct);

    public Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Attributes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<EnumValue>> GetEnumValuesAsync(int attributeId, CancellationToken ct) =>
        await _db.EnumValues.AsNoTracking()
            .Where(e => e.AttributeId == attributeId)
            .OrderBy(e => e.SortOrder).ThenBy(e => e.TitleRu)
            .ToListAsync(ct);
}
