namespace DomeoProductsDb.Application.Suppliers;

public record SupplierDto(int Id, string Name);

public record SupplierDetailDto(
    int Id,
    string Name,
    string? Email,
    string? Phone,
    string? Website,
    string? Address,
    string? Country,
    string? Inn);
