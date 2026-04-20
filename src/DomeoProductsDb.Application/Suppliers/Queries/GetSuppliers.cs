using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<List<SupplierDto>>;

public class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly ISupplierRepository _repo;

    public GetSuppliersHandler(ISupplierRepository repo) => _repo = repo;

    public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(s => new SupplierDto(s.Id, s.Name)).ToList();
    }
}
