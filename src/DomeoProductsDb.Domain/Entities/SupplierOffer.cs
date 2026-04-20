namespace DomeoProductsDb.Domain.Entities;

public class SupplierOffer
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "RUB";

    public Product? Product { get; set; }
    public Supplier? Supplier { get; set; }
}
