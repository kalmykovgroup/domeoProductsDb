namespace DomeoProductsDb.Application.Categories;

public record CategoryDto(int Id, int? ParentId, string Code, string TitleRu, bool IsLeaf);

public record CategoryNode(int Id, string Code, string TitleRu, bool IsLeaf, List<CategoryNode> Children);
