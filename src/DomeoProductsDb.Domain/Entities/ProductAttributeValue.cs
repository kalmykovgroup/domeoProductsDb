namespace DomeoProductsDb.Domain.Entities;

public class ProductAttributeValue
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int AttributeId { get; set; }

    public AttributeValueType ValueType { get; set; }

    public string? ValueText { get; set; }
    public decimal? ValueNumeric { get; set; }
    public bool? ValueBool { get; set; }
    public int? EnumValueId { get; set; }
    public int? BrandId { get; set; }

    public Product? Product { get; set; }
    public ProductAttribute? Attribute { get; set; }
    public EnumValue? EnumValue { get; set; }
    public Brand? Brand { get; set; }
}
