namespace DomeoProductsDb.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string ExternalCode { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? MainImageFilename { get; set; }

    public Category? Category { get; set; }
    public ICollection<ProductAttributeValue> Attributes { get; set; } = new List<ProductAttributeValue>();
    public ICollection<SupplierOffer> Offers { get; set; } = new List<SupplierOffer>();
}
