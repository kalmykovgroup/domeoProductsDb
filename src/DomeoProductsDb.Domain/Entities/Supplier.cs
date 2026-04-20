namespace DomeoProductsDb.Domain.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? Inn { get; set; }

    public ICollection<SupplierOffer> Offers { get; set; } = new List<SupplierOffer>();
}
