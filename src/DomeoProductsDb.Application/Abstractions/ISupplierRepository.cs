using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Abstractions;

public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct);
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct);
}
