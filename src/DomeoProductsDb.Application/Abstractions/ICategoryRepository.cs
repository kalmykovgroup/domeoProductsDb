using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);
    Task<Category?> GetByCodeAsync(string code, CancellationToken ct);
}
