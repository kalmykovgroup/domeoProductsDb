using System.Text.Json;
using System.Text.Json.Serialization;
using DomeoProductsDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DomeoProductsDb.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ProductsDbContext _db;
    private readonly IHostEnvironment _env;
    private readonly ILogger<DatabaseSeeder> _log;

    public DatabaseSeeder(ProductsDbContext db, IHostEnvironment env, ILogger<DatabaseSeeder> log)
    {
        _db = db;
        _env = env;
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.Products.AnyAsync(ct))
        {
            _log.LogInformation("Database already seeded; skipping");
            return;
        }

        var path = Path.Combine(_env.ContentRootPath, "SeedData", "seed.json");
        if (!File.Exists(path))
        {
            _log.LogWarning("Seed file missing at {Path}; skipping seed", path);
            return;
        }

        _log.LogInformation("Loading seed from {Path}", path);
        await using var stream = File.OpenRead(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
        var seed = await JsonSerializer.DeserializeAsync<SeedPayload>(stream, options, ct)
                   ?? throw new InvalidOperationException("seed.json empty");

        // Reference data — insertion order matters: categories (self-FK), then brands/suppliers/attributes/enum_values.
        // Normalize dangling parents (refs outside our dataset) to null so FK holds.
        var knownCategoryIds = seed.Categories.Select(c => c.Id).ToHashSet();
        _db.Categories.AddRange(OrderCategoriesByParent(seed.Categories).Select(c => new Category
        {
            Id = c.Id,
            ParentId = c.ParentId is int pid && knownCategoryIds.Contains(pid) ? pid : null,
            Code = c.Code,
            TitleRu = c.TitleRu,
            IsLeaf = c.IsLeaf,
        }));
        _db.Brands.AddRange(seed.Brands.Select(b => new Brand { Id = b.Id, TitleRu = b.TitleRu }));
        _db.Suppliers.AddRange(seed.Suppliers.Select(s => new Supplier { Id = s.Id, Name = s.Name }));
        _db.Attributes.AddRange(seed.Attributes.Select(a => new ProductAttribute
        {
            Id = a.Id,
            Code = a.Code,
            TitleRu = a.TitleRu,
            ValueType = a.ValueType,
        }));
        _db.EnumValues.AddRange(seed.EnumValues.Select(ev => new EnumValue
        {
            Id = ev.Id,
            AttributeId = ev.AttributeId,
            Code = ev.Code,
            TitleRu = ev.TitleRu,
            SortOrder = ev.SortOrder,
        }));
        await _db.SaveChangesAsync(ct);

        _log.LogInformation("Seeding {Count} products…", seed.Products.Count);
        const int batch = 200;
        var list = seed.Products;
        for (var i = 0; i < list.Count; i += batch)
        {
            foreach (var p in list.Skip(i).Take(batch))
            {
                var product = new Product
                {
                    Id = p.Id,
                    ExternalCode = p.ExternalCode,
                    NameRu = p.NameRu,
                    CategoryId = p.CategoryId,
                    MainImageFilename = p.MainImageFilename,
                };
                foreach (var av in p.Attributes)
                {
                    product.Attributes.Add(new ProductAttributeValue
                    {
                        AttributeId = av.AttrId,
                        ValueType = av.ValueType,
                        ValueText = av.ValueText,
                        ValueNumeric = av.ValueNumeric,
                        ValueBool = av.ValueBool,
                        EnumValueId = av.EnumValueId,
                        BrandId = av.BrandId,
                    });
                }
                foreach (var o in p.Offers)
                {
                    product.Offers.Add(new SupplierOffer
                    {
                        SupplierId = o.SupplierId,
                        PriceAmount = o.PriceAmount,
                        Currency = o.Currency ?? "RUB",
                    });
                }
                _db.Products.Add(product);
            }
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("Seeded {Done}/{Total}", Math.Min(i + batch, list.Count), list.Count);
        }
    }

    private static IEnumerable<SeedCategory> OrderCategoriesByParent(List<SeedCategory> cats)
    {
        var remaining = cats.ToList();
        var placed = new HashSet<int>();
        while (remaining.Count > 0)
        {
            var next = remaining
                .Where(c => c.ParentId is null
                         || placed.Contains(c.ParentId.Value)
                         || cats.All(x => x.Id != c.ParentId))
                .ToList();
            if (next.Count == 0) throw new InvalidOperationException("cycle in categories");
            foreach (var c in next)
            {
                placed.Add(c.Id);
                yield return c;
            }
            remaining.RemoveAll(c => placed.Contains(c.Id));
        }
    }

    private sealed class SeedPayload
    {
        public List<SeedCategory> Categories { get; set; } = new();
        public List<SeedBrand> Brands { get; set; } = new();
        public List<SeedSupplier> Suppliers { get; set; } = new();
        public List<SeedAttribute> Attributes { get; set; } = new();
        public List<SeedEnumValue> EnumValues { get; set; } = new();
        public List<SeedProduct> Products { get; set; } = new();
    }

    private sealed class SeedCategory
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; } = "";
        public string TitleRu { get; set; } = "";
        public bool IsLeaf { get; set; }
    }

    private sealed class SeedBrand { public int Id { get; set; } public string TitleRu { get; set; } = ""; }
    private sealed class SeedSupplier { public int Id { get; set; } public string Name { get; set; } = ""; }

    private sealed class SeedAttribute
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string TitleRu { get; set; } = "";
        public AttributeValueType ValueType { get; set; }
    }

    private sealed class SeedEnumValue
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string Code { get; set; } = "";
        public string TitleRu { get; set; } = "";
        public int SortOrder { get; set; }
    }

    private sealed class SeedProduct
    {
        public int Id { get; set; }
        public string ExternalCode { get; set; } = "";
        public string NameRu { get; set; } = "";
        public int CategoryId { get; set; }
        public string? MainImageFilename { get; set; }
        public List<SeedAttrValue> Attributes { get; set; } = new();
        public List<SeedOffer> Offers { get; set; } = new();
    }

    private sealed class SeedAttrValue
    {
        public int AttrId { get; set; }
        public AttributeValueType ValueType { get; set; }
        public string? ValueText { get; set; }
        public decimal? ValueNumeric { get; set; }
        public bool? ValueBool { get; set; }
        public int? EnumValueId { get; set; }
        public int? BrandId { get; set; }
    }

    private sealed class SeedOffer
    {
        public int SupplierId { get; set; }
        public decimal PriceAmount { get; set; }
        public string? Currency { get; set; }
    }
}
