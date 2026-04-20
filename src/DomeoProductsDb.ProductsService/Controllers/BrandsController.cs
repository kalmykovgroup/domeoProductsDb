using DomeoProductsDb.Application.Brands;
using DomeoProductsDb.Application.Brands.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomeoProductsDb.ProductsService.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<BrandDto>> GetAll(CancellationToken ct) =>
        _mediator.Send(new GetBrandsQuery(), ct);
}
