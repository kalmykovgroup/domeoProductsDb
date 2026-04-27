using System.Globalization;
using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Domain.Entities;
using DomeoProductsDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _db;

    public ProductRepository(ProductsDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        int? categoryId,
        string? query,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? attributeFilters,
        int page,
        int pageSize,
        CancellationToken ct)
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

        if (attributeFilters is { Count: > 0 })
        {
            // Resolve codes → attribute metadata in one round-trip.
            var codes = attributeFilters.Keys.ToList();
            var attrs = await _db.Attributes
                .AsNoTracking()
                .Where(a => codes.Contains(a.Code))
                .ToDictionaryAsync(a => a.Code, ct);

            foreach (var (code, rawValues) in attributeFilters)
            {
                if (rawValues.Count == 0) continue;

                if (!attrs.TryGetValue(code, out var attr))
                {
                    // Unknown attribute code → no product can match.
                    q = q.Where(_ => false);
                    continue;
                }

                var aid = attr.Id;
                switch (attr.ValueType)
                {
                    case AttributeValueType.Numeric:
                        var nums = rawValues
                            .Select(v => decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? (decimal?)n : null)
                            .Where(n => n.HasValue)
                            .Select(n => n!.Value)
                            .ToList();
                        if (nums.Count == 0)
                        {
                            q = q.Where(_ => false);
                        }
                        else
                        {
                            q = q.Where(p => p.Attributes.Any(av =>
                                av.AttributeId == aid && av.ValueNumeric.HasValue && nums.Contains(av.ValueNumeric.Value)));
                        }
                        break;

                    case AttributeValueType.Bool:
                        var bools = rawValues
                            .Select(v => v.Equals("true", StringComparison.OrdinalIgnoreCase)
                                      || v.Equals("да", StringComparison.OrdinalIgnoreCase)
                                      || v == "1")
                            .Distinct()
                            .ToList();
                        q = q.Where(p => p.Attributes.Any(av =>
                            av.AttributeId == aid && av.ValueBool.HasValue && bools.Contains(av.ValueBool.Value)));
                        break;

                    case AttributeValueType.Enum:
                        var enumCodes = rawValues.ToList();
                        q = q.Where(p => p.Attributes.Any(av =>
                            av.AttributeId == aid && av.EnumValue != null && enumCodes.Contains(av.EnumValue.Code)));
                        break;

                    case AttributeValueType.Brand:
                        var brandIds = rawValues
                            .Select(v => int.TryParse(v, out var b) ? (int?)b : null)
                            .Where(b => b.HasValue)
                            .Select(b => b!.Value)
                            .ToList();
                        if (brandIds.Count == 0)
                        {
                            q = q.Where(_ => false);
                        }
                        else
                        {
                            q = q.Where(p => p.Attributes.Any(av =>
                                av.AttributeId == aid && av.BrandId.HasValue && brandIds.Contains(av.BrandId.Value)));
                        }
                        break;

                    case AttributeValueType.Text:
                        var texts = rawValues.ToList();
                        q = q.Where(p => p.Attributes.Any(av =>
                            av.AttributeId == aid && av.ValueText != null && texts.Contains(av.ValueText)));
                        break;
                }
            }
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
