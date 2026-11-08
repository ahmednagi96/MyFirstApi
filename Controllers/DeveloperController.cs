namespace MyFirstApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyFirstApi.Entities;

[ApiController]
[Route("[controller]")]
public class DeveloperController : ControllerBase
{
    private readonly Developer _developer;
    public DeveloperController(IOptions<Developer> developer)
    {
        _developer = developer.Value;
    }
    [HttpGet("get-developer")]
    public IActionResult Get()
    {
        return Ok($"Name: {_developer.Name}, Age: {_developer.Age}, Email: {_developer.Email}, Phone: {_developer.Phone}");
    }
}
