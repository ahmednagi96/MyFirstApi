using Microsoft.AspNetCore.Mvc;
using MyFirstApi.DTOs.Categories.Requests;
using MyFirstApi.DTOs.Categories.Responses;
using MyFirstApi.Services;
using Microsoft.AspNetCore.Authorization;
using PermissionConstants = MyFirstApi.Domain.Constants.Permissions;

namespace MyFirstApi.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
  [HttpGet]
  [ProducesResponseType(typeof(IEnumerable<CategoryDetailsDto>), StatusCodes.Status200OK)]
  [Authorize(Policy = PermissionConstants.Categories.View)]
  public async Task<ActionResult<IEnumerable<CategoryDetailsDto>>> GetAll(CancellationToken ct)
    => await categoryService.GetAllAsync(ct);

    [HttpGet("{id:int:min(1)}")]
  [ProducesResponseType(typeof(CategoryDetailsDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CategoryDetailsDto>> GetById(int id, CancellationToken ct)
    => await categoryService.GetByIdAsync(id, ct);

  [HttpPost]
  [ProducesResponseType(typeof(CategoryDetailsDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [Authorize(Policy = PermissionConstants.Categories.Create)]
  public async Task<ActionResult<CategoryDetailsDto>> Create(
    [FromBody] CreateCategoryRequestDto dto,
    CancellationToken ct)
  {
    var created = await categoryService.CreateAsync(dto, ct);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }
}
