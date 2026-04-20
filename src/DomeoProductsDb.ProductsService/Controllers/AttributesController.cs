using DomeoProductsDb.Application.Attributes;
using DomeoProductsDb.Application.Attributes.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomeoProductsDb.ProductsService.Controllers;

[ApiController]
[Route("api/attributes")]
public class AttributesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttributesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<AttributeDto>> GetAll(CancellationToken ct) =>
        _mediator.Send(new GetAttributesQuery(), ct);

    [HttpGet("{id:int}/enum-values")]
    public async Task<ActionResult<List<EnumValueDto>>> GetEnumValues(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAttributeEnumValuesQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
