namespace DomeoProductsDb.Domain.Entities;

public class ProductAttribute
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TitleRu { get; set; } = string.Empty;
    public AttributeValueType ValueType { get; set; }

    public ICollection<EnumValue> EnumValues { get; set; } = new List<EnumValue>();
}
