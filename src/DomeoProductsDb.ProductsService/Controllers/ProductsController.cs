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
        CancellationToken ct = default) =>
        _mediator.Send(new SearchProductsQuery(categoryId, q, page, pageSize), ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
