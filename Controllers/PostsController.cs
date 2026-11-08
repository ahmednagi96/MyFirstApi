using Microsoft.AspNetCore.Mvc;
using MyFirstApi.Commands;
using MyFirstApi.DTOs.Posts.Responses;
using MediatR;
using MyFirstApi.Queries.GetPostById;
using Microsoft.AspNetCore.Authorization;

namespace MyFirstApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PostsController(IMediator mediator
    //ICurrentUser currentUser,
) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDetailsDto>> Get([FromRoute] int id, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetPostByIdQuery(id), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostDetailsDto>> Create(
        [FromBody] CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(
           command,
        cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePostCommand(id), cancellationToken);
        return NoContent();
    }
}
