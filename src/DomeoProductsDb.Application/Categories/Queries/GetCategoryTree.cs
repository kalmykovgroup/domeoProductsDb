using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Categories.Queries;

public record GetCategoryTreeQuery : IRequest<List<CategoryNode>>;

public class GetCategoryTreeHandler : IRequestHandler<GetCategoryTreeQuery, List<CategoryNode>>
{
    private readonly ICategoryRepository _repo;

    public GetCategoryTreeHandler(ICategoryRepository repo) => _repo = repo;

    public async Task<List<CategoryNode>> Handle(GetCategoryTreeQuery request, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        var byParent = all.ToLookup(c => c.ParentId);

        List<CategoryNode> Build(int? parentId) =>
            byParent[parentId]
                .OrderBy(c => c.TitleRu)
                .Select(c => new CategoryNode(c.Id, c.Code, c.TitleRu, c.IsLeaf, Build(c.Id)))
                .ToList();

        var roots = all.Where(c => c.ParentId == null || all.All(p => p.Id != c.ParentId))
                       .Select(c => c.Id)
                       .ToHashSet();

        return all
            .Where(c => roots.Contains(c.Id))
            .OrderBy(c => c.TitleRu)
            .Select(c => new CategoryNode(c.Id, c.Code, c.TitleRu, c.IsLeaf, Build(c.Id)))
            .ToList();
    }
}
