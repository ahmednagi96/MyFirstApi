using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyFirstApi.Filters;

namespace MyFirstApi.Controllers;


public class TasksController(IMemoryCache _cache) : ApiControllerBase
{
    [HttpGet("get-tasks/{name}", Name = "GetTasks")]
    public IActionResult GetTasks(string name)
    {
        if(!name.Contains("test-name"))
        {
            return NotFoundResponse("Task", name);
        }
        return Ok(GetData(name: $"Task 1 {name}"));
    }

    [HttpGet("test-link")]
    public string GetTasks2()
    {
       var url = Url.Link("GetTasks",new { name = "Task1" , id = 1 });
       return $"{url}";
    }


    [NonAction]
    private string GetData(string name)
    {
       return $"{name}";
    }

    [HttpHead("custom-header")]
    public IActionResult HeadTasks()
    {
        //Response.Headers.Add("X-Custom-Header", "Hello from custom header");
        return NoContent();
    }
    [HttpGet("search")]
    [HttpGet("find")]
    public IActionResult FindTasks( [FromQuery] string search)
    {
        if (!search.Contains("search-key"))
        {
            return BadRequest("Search key is required and must contain 'search-key'");
        }
        return Ok($"Found {search} in the database");
    }

    [HttpGet("~/health")]
    public IActionResult HealthCheck()
    {
        return Ok("Health check OK");
    }


    [HttpGet("test-done")]
    public IActionResult TestDone([FromHeader(Name = "X-Custom-Header")] string customHeader)
    {
        return Ok($"Test done {customHeader}");
    }

    [HttpGet("test-list")]
    public IActionResult TestList([FromQuery] List<string> names)
    {
        return Ok($"Test list {string.Join(", ", names)}");
    }

    [HttpGet("test/{name}")]
    public IActionResult TestName([FromRoute] string name)
    {
        if(!name.Contains("test-name"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Forbidden");
        }
        return Ok($"Test name {name}");
    }


    [HttpGet("test-cache")]
    public IActionResult TestCacheValue()
    {
      // var result=CacheFilter.cache.TryGetValue("/api/Middlewares/cache", out var cachedResult);
       if (_cache.TryGetValue("/api/Middlewares/cache", out string? cachedResult))
        {
            return Ok(cachedResult);
        }
       return NotFound("Test cache not found");
    }
}