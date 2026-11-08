using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected NotFoundObjectResult NotFoundResponse(string resource, object id)
        => NotFound(new { message = $"{resource} with id {id} was not found." });
}