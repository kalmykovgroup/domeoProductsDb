using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Suppliers.Queries;

public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDetailDto?>;

public class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDetailDto?>
{
    private readonly ISupplierRepository _repo;

    public GetSupplierByIdHandler(ISupplierRepository repo) => _repo = repo;

    public async Task<SupplierDetailDto?> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(request.Id, ct);
        if (s is null) return null;
        return new SupplierDetailDto(
            s.Id, s.Name, s.Email, s.Phone, s.Website, s.Address, s.Country, s.Inn);
    }
}
