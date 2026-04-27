using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Abstractions;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        int? categoryId,
        string? query,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? attributeFilters,
        int page,
        int pageSize,
        CancellationToken ct);

    Task<Product?> GetDetailAsync(int id, CancellationToken ct);
}
