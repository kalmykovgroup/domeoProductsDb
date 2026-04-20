using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Attributes.Queries;

public record GetAttributesQuery : IRequest<List<AttributeDto>>;

public class GetAttributesHandler : IRequestHandler<GetAttributesQuery, List<AttributeDto>>
{
    private readonly IAttributeRepository _repo;

    public GetAttributesHandler(IAttributeRepository repo) => _repo = repo;

    public async Task<List<AttributeDto>> Handle(GetAttributesQuery request, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(a => new AttributeDto(a.Id, a.Code, a.TitleRu, a.ValueType)).ToList();
    }
}
