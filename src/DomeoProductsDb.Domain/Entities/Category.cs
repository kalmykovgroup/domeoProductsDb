namespace DomeoProductsDb.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TitleRu { get; set; } = string.Empty;
    public bool IsLeaf { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
