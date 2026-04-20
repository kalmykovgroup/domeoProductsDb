using DomeoProductsDb.Application.Suppliers;
using DomeoProductsDb.Application.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomeoProductsDb.ProductsService.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<SupplierDto>> GetAll(CancellationToken ct) =>
        _mediator.Send(new GetSuppliersQuery(), ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDetailDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
