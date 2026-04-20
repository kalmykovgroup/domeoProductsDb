using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Abstractions;

public interface IBrandRepository
{
    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken ct);
}
