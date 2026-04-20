using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Abstractions;

public interface IAttributeRepository
{
    Task<IReadOnlyList<ProductAttribute>> GetAllAsync(CancellationToken ct);
    Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EnumValue>> GetEnumValuesAsync(int attributeId, CancellationToken ct);
}
