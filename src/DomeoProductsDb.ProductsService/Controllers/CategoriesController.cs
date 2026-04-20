using DomeoProductsDb.Application.Categories;
using DomeoProductsDb.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomeoProductsDb.ProductsService.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<CategoryDto>> GetAll(CancellationToken ct) =>
        _mediator.Send(new GetCategoriesQuery(), ct);

    [HttpGet("tree")]
    public Task<List<CategoryNode>> GetTree(CancellationToken ct) =>
        _mediator.Send(new GetCategoryTreeQuery(), ct);
}
