using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Attributes.Queries;

public record GetAttributeEnumValuesQuery(int AttributeId) : IRequest<List<EnumValueDto>?>;

public class GetAttributeEnumValuesHandler
    : IRequestHandler<GetAttributeEnumValuesQuery, List<EnumValueDto>?>
{
    private readonly IAttributeRepository _repo;

    public GetAttributeEnumValuesHandler(IAttributeRepository repo) => _repo = repo;

    public async Task<List<EnumValueDto>?> Handle(GetAttributeEnumValuesQuery request, CancellationToken ct)
    {
        var attribute = await _repo.GetByIdAsync(request.AttributeId, ct);
        if (attribute is null) return null; // → 404

        var values = await _repo.GetEnumValuesAsync(request.AttributeId, ct);
        return values
            .Select(v => new EnumValueDto(v.Id, v.AttributeId, v.Code, v.TitleRu, v.SortOrder))
            .ToList();
    }
}
