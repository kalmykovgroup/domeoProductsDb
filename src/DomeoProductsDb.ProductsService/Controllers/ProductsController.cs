using DomeoProductsDb.Application.Common;
using DomeoProductsDb.Application.Products;
using DomeoProductsDb.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomeoProductsDb.ProductsService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<PagedResult<ProductSummaryDto>> Search(
        [FromQuery] int? categoryId,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        // Parse dynamic attribute filters: "attr.<code>=<value>".
        // Поддерживаются два способа задания нескольких значений одного атрибута (OR):
        //   1) Повторение query-параметра: ?attr.толщина=16&attr.толщина=18
        //   2) Запятая-разделитель: ?attr.толщина=16,18
        // Между разными атрибутами — AND. Пример комбо:
        //   ?attr.толщина=16,18&attr.страна_сборки=германия
        var attrFilters = HttpContext.Request.Query
            .Where(kv => kv.Key.StartsWith("attr.", StringComparison.Ordinal) && kv.Key.Length > 5)
            .ToDictionary(
                kv => kv.Key[5..],
                kv =>
                {
                    // StringValues уже даёт массив при repeated key. Дополнительно разбиваем по запятой.
                    var parts = new List<string>();
                    foreach (var raw in kv.Value)
                    {
                        if (string.IsNullOrEmpty(raw)) continue;
                        foreach (var piece in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            if (!string.IsNullOrWhiteSpace(piece)) parts.Add(piece);
                        }
                    }
                    return (IReadOnlyList<string>)parts;
                },
                StringComparer.Ordinal);

        // Уберём атрибуты с пустым списком значений после очистки.
        var nonEmpty = attrFilters
            .Where(kv => kv.Value.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        return _mediator.Send(
            new SearchProductsQuery(categoryId, q,
                nonEmpty.Count == 0 ? null : nonEmpty,
                page, pageSize),
            ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
