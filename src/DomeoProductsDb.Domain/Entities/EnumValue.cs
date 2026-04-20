namespace DomeoProductsDb.Domain.Entities;

public class EnumValue
{
    public int Id { get; set; }
    public int AttributeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TitleRu { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ProductAttribute? Attribute { get; set; }
}
