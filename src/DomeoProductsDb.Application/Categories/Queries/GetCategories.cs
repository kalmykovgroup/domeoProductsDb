using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _repo;

    public GetCategoriesHandler(ICategoryRepository repo) => _repo = repo;

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(c => new CategoryDto(c.Id, c.ParentId, c.Code, c.TitleRu, c.IsLeaf)).ToList();
    }
}
