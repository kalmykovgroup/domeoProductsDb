using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Brands.Queries;

public record GetBrandsQuery : IRequest<List<BrandDto>>;

public class GetBrandsHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IBrandRepository _repo;

    public GetBrandsHandler(IBrandRepository repo) => _repo = repo;

    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(b => new BrandDto(b.Id, b.TitleRu)).ToList();
    }
}
