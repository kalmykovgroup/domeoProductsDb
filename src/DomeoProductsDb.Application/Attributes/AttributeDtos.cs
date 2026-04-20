using DomeoProductsDb.Domain.Entities;

namespace DomeoProductsDb.Application.Attributes;

public record AttributeDto(
    int Id,
    string Code,
    string TitleRu,
    AttributeValueType ValueType);

public record EnumValueDto(
    int Id,
    int AttributeId,
    string Code,
    string TitleRu,
    int SortOrder);
