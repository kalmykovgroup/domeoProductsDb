using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Products;

public record ProductSummaryDto(
    int Id,
    string ExternalCode,
    string NameRu,
    int CategoryId,
    string CategoryTitle,
    decimal? MinPrice,
    string? MainSupplier,
    string? PreviewUrl);

public record OfferDto(int SupplierId, string SupplierName, decimal PriceAmount, string Currency);

public record AttributeValueDto(
    int AttributeId,
    string Code,
    string TitleRu,
    AttributeValueType ValueType,
    string? ValueText,
    decimal? ValueNumeric,
    bool? ValueBool,
    int? EnumValueId,
    string? EnumCode,
    string? EnumTitleRu,
    int? BrandId,
    string? BrandTitleRu);

public record ProductDetailDto(
    int Id,
    string ExternalCode,
    string NameRu,
    int CategoryId,
    string CategoryTitle,
    string? PreviewUrl,
    string? FullUrl,
    IReadOnlyList<OfferDto> Offers,
    IReadOnlyList<AttributeValueDto> Attributes);
