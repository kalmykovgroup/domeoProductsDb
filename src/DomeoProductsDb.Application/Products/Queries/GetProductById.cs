using DomeoProductsDb.Application.Abstractions;
using MediatR;

namespace DomeoProductsDb.Application.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto?>;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly IProductRepository _repo;

    public GetProductByIdHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var p = await _repo.GetDetailAsync(request.Id, ct);
        if (p is null) return null;

        var offers = p.Offers
            .OrderBy(o => o.PriceAmount)
            .Select(o => new OfferDto(o.SupplierId, o.Supplier?.Name ?? "", o.PriceAmount, o.Currency))
            .ToList();

        var attrs = p.Attributes
            .OrderBy(a => a.Attribute?.TitleRu)
            .Select(a => new AttributeValueDto(
                AttributeId:   a.AttributeId,
                Code:          a.Attribute?.Code ?? string.Empty,
                TitleRu:       a.Attribute?.TitleRu ?? string.Empty,
                ValueType:     a.ValueType,
                ValueText:     a.ValueText,
                ValueNumeric:  a.ValueNumeric,
                ValueBool:     a.ValueBool,
                EnumValueId:   a.EnumValueId,
                EnumCode:      a.EnumValue?.Code,
                EnumTitleRu:   a.EnumValue?.TitleRu,
                BrandId:       a.BrandId,
                BrandTitleRu:  a.Brand?.TitleRu))
            .ToList();

        var previewUrl = p.MainImageFilename is null ? null : $"/images/preview/{p.MainImageFilename}";
        var fullUrl    = p.MainImageFilename is null ? null : $"/images/full/{p.MainImageFilename}";

        return new ProductDetailDto(
            p.Id, p.ExternalCode, p.NameRu, p.CategoryId,
            p.Category?.TitleRu ?? string.Empty,
            previewUrl, fullUrl,
            offers, attrs);
    }
}
