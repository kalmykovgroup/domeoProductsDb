using DomeoProductsDb.Application.Common;
using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Products.Queries;

public record SearchProductsQuery(
    int? CategoryId,
    string? Query,
    int Page,
    int PageSize) : IRequest<PagedResult<ProductSummaryDto>>;

public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, PagedResult<ProductSummaryDto>>
{
    private readonly IProductRepository _repo;

    public SearchProductsHandler(IProductRepository repo) => _repo = repo;

    public async Task<PagedResult<ProductSummaryDto>> Handle(SearchProductsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var (items, total) = await _repo.SearchAsync(
            request.CategoryId, request.Query, page, pageSize, ct);

        var dtos = items.Select(p =>
        {
            var minOffer = p.Offers.OrderBy(o => o.PriceAmount).FirstOrDefault();
            return new ProductSummaryDto(
                p.Id,
                p.ExternalCode,
                p.NameRu,
                p.CategoryId,
                p.Category?.TitleRu ?? string.Empty,
                minOffer?.PriceAmount,
                minOffer?.Supplier?.Name,
                PreviewUrl: p.MainImageFilename is null
                    ? null
                    : $"/images/preview/{p.MainImageFilename}");
        }).ToList();

        return new PagedResult<ProductSummaryDto>(dtos, total, page, pageSize);
    }
}
