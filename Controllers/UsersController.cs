using Microsoft.AspNetCore.Mvc;
using MyFirstApi.Services;
using MyFirstApi.DTOs.Users.Requests;
using MyFirstApi.DTOs.Users.Responses;  

namespace MyFirstApi.Controllers;

public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll(
        [FromQuery] UserFilterDto filter,
        CancellationToken ct)
        => Ok(await userService.GetAllAsync(filter, ct));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> GetById(int id, CancellationToken ct)
        => Ok(await userService.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailsDto>> Create(
        [FromBody] CreateUserRequestDto dto,
        CancellationToken ct)
    {
        var created = await userService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequestDto dto,
        CancellationToken ct)
    {
        await userService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await userService.DeleteAsync(id, ct);
        return NoContent();
    }
}
